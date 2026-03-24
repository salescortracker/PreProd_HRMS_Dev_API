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
    public class ExpenseStatusService : IExpenseStatusService
    {
        private readonly HRMSContext _context;

        public ExpenseStatusService(HRMSContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExpenseStatusDto>> GetExpenseStatus(int userId)
        {
            return await _context.ExpenseStatuses
                .Where(x=>x.UserId==userId)
                .Select(x => new ExpenseStatusDto
                {
                    ExpenseStatusID = x.ExpenseStatusId,
                    ExpenseStatusName = x.ExpenseStatusName,
                    IsActive = x.IsActive,
                    CompanyID = x.CompanyId,
                    RegionID = x.RegionId
                })
                .ToListAsync();
        }

        public async Task<ExpenseStatusDto> CreateExpenseStatus(ExpenseStatusDto dto)
        {
            var entity = new ExpenseStatus
            {
                ExpenseStatusName = dto.ExpenseStatusName,
                IsActive = dto.IsActive,
                CreatedBy= dto.UserId,
                UserId = dto.UserId,      
                CompanyId = dto.CompanyID,
                RegionId = dto.RegionID
            };

            _context.ExpenseStatuses.Add(entity);
            await _context.SaveChangesAsync();

            dto.ExpenseStatusID = entity.ExpenseStatusId;

            return dto;
        }

        public async Task<ExpenseStatusDto> UpdateExpenseStatus(ExpenseStatusDto dto)
        {
            var entity = await _context.ExpenseStatuses
                .FirstOrDefaultAsync(x => x.ExpenseStatusId == dto.ExpenseStatusID);

            if (entity == null)
                return null;

            entity.ExpenseStatusName = dto.ExpenseStatusName;
            entity.IsActive = dto.IsActive;
            entity.CompanyId = dto.CompanyID;
            entity.RegionId = dto.RegionID;

            await _context.SaveChangesAsync();

            return dto;
        }

        public async Task<bool> DeleteExpenseStatus(int id)
        {
            var entity = await _context.ExpenseStatuses.FindAsync(id);

            if (entity == null)
                return false;

            _context.ExpenseStatuses.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }
    
    }
}
