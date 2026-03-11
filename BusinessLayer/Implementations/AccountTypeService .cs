using BusinessLayer.Common;
using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using DataAccessLayer.Repositories.GeneralRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Implementations
{
    public class AccountTypeService : IAccountTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET ALL
        public async Task<ApiResponse<IEnumerable<AccountTypeDto>>> GetAll(int userId)
        {
            var list = (await _unitOfWork.Repository<AccountType>()
                .FindAsync(x => !x.IsDeleted && x.CreatedBy == userId))
                .OrderByDescending(x => x.AccountTypeId)
                .ToList();

            var dto = list.Select(x => new AccountTypeDto
            {
                AccountTypeId = x.AccountTypeId,
                CompanyId = x.CompanyId,
                RegionId = x.RegionId,
                AccountType1 = x.AccountType1,
                Description = x.Description,
                IsActive = x.IsActive
            });

            return new ApiResponse<IEnumerable<AccountTypeDto>>(dto, "Account Types retrieved successfully.");
        }

        // GET BY ID
        public async Task<ApiResponse<AccountTypeDto?>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<AccountType>().GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return new ApiResponse<AccountTypeDto?>(null, "Account Type not found.", false);

            var dto = new AccountTypeDto
            {
                AccountTypeId = entity.AccountTypeId,
                CompanyId = entity.CompanyId,
                RegionId = entity.RegionId,
                AccountType1 = entity.AccountType1,
                Description = entity.Description,
                IsActive = entity.IsActive
            };

            return new ApiResponse<AccountTypeDto?>(dto, "Account Type retrieved successfully.");
        }

        // CREATE
        public async Task<ApiResponse<string>> CreateAsync(AccountTypeDto dto)
        {
            var duplicate = (await _unitOfWork.Repository<AccountType>().FindAsync(x =>
                !x.IsDeleted &&
                x.CompanyId == dto.CompanyId &&
                x.RegionId == dto.RegionId &&
                x.AccountType1.ToLower() == dto.AccountType1.ToLower()))
                .Any();

            if (duplicate)
                return new ApiResponse<string>(null!, "Duplicate Account Type exists.", false);

            var entity = new AccountType
            {
                CompanyId = dto.CompanyId,
                RegionId = dto.RegionId,
                AccountType1 = dto.AccountType1,
                Description = dto.Description,
                IsActive = dto.IsActive,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.UserId
            };

            await _unitOfWork.Repository<AccountType>().AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<string>("Account Type created successfully.");
        }

        // UPDATE
        public async Task<ApiResponse<string>> UpdateAsync(AccountTypeDto dto)
        {
            var entity = await _unitOfWork.Repository<AccountType>()
                .GetByIdAsync(dto.AccountTypeId);

            if (entity == null || entity.IsDeleted)
                return new ApiResponse<string>(null!, "Account Type not found.", false);

            entity.CompanyId = dto.CompanyId;
            entity.RegionId = dto.RegionId;
            entity.AccountType1 = dto.AccountType1;
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;
            entity.ModifiedAt = DateTime.UtcNow;
            entity.ModifiedBy = dto.UserId;

            _unitOfWork.Repository<AccountType>().Update(entity);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<string>("Account Type updated successfully.");
        }

        // DELETE
        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<AccountType>().GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return new ApiResponse<string>(null!, "Account Type not found.", false);

            entity.IsDeleted = true;
            entity.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.Repository<AccountType>().Update(entity);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<string>("Account Type deleted successfully.");
        }
    }
}
