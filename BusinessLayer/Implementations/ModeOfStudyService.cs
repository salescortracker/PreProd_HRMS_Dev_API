using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Implementations
{
    public class ModeOfStudyService:IModeOfStudyService
    {
        private readonly HRMSContext _context;
        //private readonly UnitOfWork _unitOfWork;

        public ModeOfStudyService(HRMSContext context)
        {
            _context = context;


        }

        public async Task<IEnumerable<ModeOfStudyDto>> GetAllModeOfStudtAsync(int userId)
        {
            return await _context.ModeOfStudies
                .Where(x => !x.IsDeleted && x.UserId == userId)
                .Select(x => new ModeOfStudyDto
                {
                    ModeOfStudyId = x.ModeOfStudyId,
                    ModeName = x.ModeName,
                    CompanyId = x.CompanyId,
                    RegionId = x.RegionId,
                    CompanyName = x.CompanyId != null ? _context.Companies.Where(c => c.CompanyId == x.CompanyId).FirstOrDefault().CompanyName : null,
                    RegionName = x.RegionId != null ? _context.Regions.Where(r => r.RegionId == x.RegionId).FirstOrDefault().RegionName : null,
                    IsActive = x.IsActive,
                    UserId = x.UserId
                }).ToListAsync();
        }

        // ✅ GET BY ID
        public async Task<ModeOfStudyDto?> GetByIdModeOfStudtAsync(int id)
        {
            var entity = await _context.ModeOfStudies
                .FirstOrDefaultAsync(x => x.ModeOfStudyId == id && !x.IsDeleted);

            if (entity == null) return null;

            return new ModeOfStudyDto
            {
                ModeOfStudyId = entity.ModeOfStudyId,
                ModeName = entity.ModeName,
                IsActive = entity.IsActive
            };
        }

        // ✅ CREATE
        public async Task<bool> CreateModeOfStudtAsync(ModeOfStudyDto dto)
        {
            var entity = new ModeOfStudy
            {
                ModeName = dto.ModeName,
                CompanyId = dto.CompanyId,
                RegionId = dto.RegionId,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = dto.UserId,
                CreatedAt = DateTime.Now,
                UserId = dto.UserId
            };

            _context.ModeOfStudies.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        // ✅ UPDATE
        public async Task<bool> UpdateModeOfStudtAsync(ModeOfStudyDto dto)
        {
            var entity = await _context.ModeOfStudies
                .FirstOrDefaultAsync(x => x.ModeOfStudyId == dto.ModeOfStudyId && x.UserId == dto.UserId && !x.IsDeleted);

            if (entity == null) return false;

            entity.ModeName = dto.ModeName;
            entity.CompanyId = dto.CompanyId;
            entity.RegionId = dto.RegionId;
            entity.IsActive = dto.IsActive;
            entity.ModifiedBy = dto.UserId;
            entity.ModifiedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        // ✅ DELETE (Soft Delete)
        public async Task<bool> DeleteModeOfStudtAsync(int id)
        {
            var entity = await _context.ModeOfStudies
                .FirstOrDefaultAsync(x =>
                    x.ModeOfStudyId == id &&
                    
                    !x.IsDeleted);

            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.ModifiedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }


    }
}
