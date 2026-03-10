using System;
using BusinessLayer.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface ICompanyNewsCategoryService
    {
        Task<List<CategoryDto>> GetAllCompanyNewsCategoryAsync(int userId);
        Task<CategoryDto?> GetByIdCompanyNewsCategoryAsync(int categoryId);
        Task<CategoryDto> CreateCompanyNewsCategoryAsync(CategoryDto dto);
        Task<bool> UpdateCompanyNewsCategoryAsync(int categoryId, CategoryDto dto);
        Task<bool> DeleteCompanyNewsCategoryAsync(int categoryId);
        Task<List<CategoryDto>> GetCategoriesByCompanyRegion(int companyId, int regionId);
    }
}