using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Implementations
{
    public class CompanyNewsCategoryService : ICompanyNewsCategoryService
    {
        private readonly HRMSContext _context;

        public CompanyNewsCategoryService(HRMSContext context)
        {
            _context = context;
        }

        // GET ALL
        public async Task<List<CategoryDto>> GetAllCompanyNewsCategoryAsync(int userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    CompanyId = c.CompanyId,
                    RegionId = c.RegionId,
                    CategoryName = c.CategoryName,
                    IsActive = c.IsActive,
                    IsDeleted = c.IsDeleted,
                    CreatedBy = c.CreatedBy,
                    CreatedAt = c.CreatedAt,
                    ModifiedBy = c.ModifiedBy,
                    ModifiedAt = c.ModifiedAt,
                    UserId = c.UserId,
                    CompanyName = _context.Companies.Where(co => co.CompanyId == c.CompanyId).Select(co => co.CompanyName).FirstOrDefault(),
                    RegionName = _context.Regions.Where(r => r.RegionId == c.RegionId).Select(r => r.RegionName).FirstOrDefault()
                })
                .ToListAsync();
        }

        // GET BY ID
        public async Task<CategoryDto?> GetByIdCompanyNewsCategoryAsync(int categoryId)
        {
            var category = await _context.Categories
                .Where(c => c.CategoryId == categoryId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (category == null) return null;

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                CompanyId = category.CompanyId,
                RegionId = category.RegionId,
                CategoryName = category.CategoryName,
                IsActive = category.IsActive,
                IsDeleted = category.IsDeleted,
                CreatedBy = category.CreatedBy,
                CreatedAt = category.CreatedAt,
                ModifiedBy = category.ModifiedBy,
                ModifiedAt = category.ModifiedAt,
                UserId = category.UserId
            };
        }

        // CREATE
        public async Task<CategoryDto> CreateCompanyNewsCategoryAsync(CategoryDto dto)
        {
            var entity = new Category
            {
                CompanyId = dto.CompanyId,
                RegionId = dto.RegionId,
                CategoryName = dto.CategoryName,
                IsActive = dto.IsActive,
                IsDeleted = false,
                CreatedBy = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                UserId = dto.UserId
            };

            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();

            dto.CategoryId = entity.CategoryId;
            dto.CreatedAt = entity.CreatedAt;

            return dto;
        }

        // UPDATE
        public async Task<bool> UpdateCompanyNewsCategoryAsync(int categoryId, CategoryDto dto)
        {
            var entity = await _context.Categories.FindAsync(categoryId);

            if (entity == null || entity.IsDeleted)
                return false;

            entity.CompanyId = dto.CompanyId;
            entity.RegionId = dto.RegionId;
            entity.CategoryName = dto.CategoryName;
            entity.IsActive = dto.IsActive;
            entity.ModifiedBy = dto.UserId;
            entity.ModifiedAt = DateTime.UtcNow;

            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        // DELETE (PERMANENT)
        public async Task<bool> DeleteCompanyNewsCategoryAsync(int categoryId)
        {
            var entity = await _context.Categories.FindAsync(categoryId);

            if (entity == null)
                return false;

            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<CategoryDto>> GetCategoriesByCompanyRegion(int companyId, int regionId)
        {
            return await _context.Categories
                .Where(c => c.CompanyId == companyId
                         && c.RegionId == regionId
                         && !c.IsDeleted
                         && c.IsActive)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    CompanyId = c.CompanyId,
                    RegionId = c.RegionId,
                    CategoryName = c.CategoryName,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

    }
}