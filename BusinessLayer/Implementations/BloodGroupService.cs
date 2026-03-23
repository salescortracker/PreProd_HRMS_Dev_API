using BusinessLayer.Common;
using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using DataAccessLayer.Repositories.GeneralRepository;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Implementations
{
    public class BloodGroupService: IBloodGroupService
    {

        private readonly HRMSContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public BloodGroupService(IUnitOfWork unitOfWork, HRMSContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _context = context;
        }

        #region Get All By Company
        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>>
            GetAllAsync(int companyId)
        {
            try
            {
                if (companyId <= 0)
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        null,
                        "Invalid company id",
                        false
                    );

                var data = await _unitOfWork
                    .Repository<BloodGroup>()
                    .GetAllAsync();

                var result = data
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                    .Select(MapToDto)
                    .ToList();

                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    result,
                    "Blood groups fetched successfully",
                    true
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    null,
                    ex.Message,
                    false
                );
            }
        }
        #endregion
        
        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>>
            GetAllCmpRegAsync(int companyId,int regionId)
        {
            try
            {
                if (companyId <= 0)
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        null,
                        "Invalid company id",
                        false
                    );

                var data = await _unitOfWork
                    .Repository<BloodGroup>()
                    .GetAllAsync();

                var result = data
                    .Where(x => x.CompanyId == companyId && x.RegionId==regionId && !x.IsDeleted)
                    .Select(MapToDto)
                    .ToList();

                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    result,
                    "Blood groups fetched successfully",
                    true
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    null,
                    ex.Message,
                    false
                );
            }
        }
        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetAlluserIdAsync(int userId)
        {
            try
            {
                var result = await _context.BloodGroups
                    .Where(x => x.UserId == userId && x.IsDeleted == false)
                    .Select(x => new BloodGroupDto
                    {
                        BloodGroupID = x.BloodGroupId,
                        BloodGroupName = x.BloodGroupName,
                        CompanyID = x.CompanyId,
                        RegionID = x.RegionId,
                        Description = x.Description,
                        IsActive = x.IsActive,
                        UserID = x.UserId ?? 0
                    })
                    .ToListAsync();

                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    result,
                    "Blood groups fetched successfully",
                    true
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<BloodGroupDto>>(null, ex.Message, false);
            }
        }




        #region Get By Id
        public async Task<ApiResponse<IEnumerable<BloodGroupDto>?>>
            GetByIdAsync(int id)
        {
            try
            {
                var entity =  _context.BloodGroups.Where(x => x.UserId == id).Select(MapToDto).ToList();

                if (entity == null)
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        null,
                        "Blood group not found",
                        true
                    );

                var dto = (entity);

                
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        dto,
                        "Blood group Fetched Successfully",
                        true
                    );
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    null,
                    ex.Message,
                    false
                );
            }
        }
        #endregion



        #region Create
        public async Task<ApiResponse<BloodGroupDto>>
            CreateAsync(BloodGroupDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.BloodGroupName))
                    return new ApiResponse<BloodGroupDto>(
                        null,
                        "Blood group name is required",
                        false
                    );

                // 🔥 Duplicate Check
                var existing = (await _unitOfWork
                    .Repository<BloodGroup>()
                    .GetAllAsync())
                    .FirstOrDefault(x =>
                        x.BloodGroupName.ToLower() ==
                        dto.BloodGroupName.ToLower() &&
                        x.CompanyId == dto.CompanyID && !x.IsDeleted && x.UserId == dto.UserID
                       );

                if (existing != null)
                    return new ApiResponse<BloodGroupDto>(
                        null,
                        "Blood group already exists",
                        false
                    );

                var entity = new BloodGroup
                {
                    BloodGroupName = dto.BloodGroupName.Trim(),
                    CompanyId = dto.CompanyID,
                    RegionId = dto.RegionID,
                    IsActive = dto.IsActive,
                    IsDeleted = false,
                    CreatedBy = dto.UserID,
                    CreatedAt = DateTime.Now,
                    UserId = dto.UserID,

                };

                await _unitOfWork.Repository<BloodGroup>().AddAsync(entity);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<BloodGroupDto>(
                    MapToDto(entity),
                    "Blood group created successfully",
                    true
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodGroupDto>(
                    null,
                    ex.Message,
                    false
                );
            }
        }
        #endregion


        #region Update
        public async Task<ApiResponse<BloodGroupDto>> UpdateAsync(BloodGroupDto dto)
        {
            try
            {
                var entity = await _context.BloodGroups
                    .FirstOrDefaultAsync(x => x.BloodGroupId == dto.BloodGroupID);

                if (entity == null)
                    return new ApiResponse<BloodGroupDto>(null, "Blood group not found", false);

                // 🔥 Normalize input
                var normalizedName = dto.BloodGroupName.Trim().ToUpper();

                // 🔥 Perfect duplicate check
                var duplicate = await _context.BloodGroups.AnyAsync(x =>
                    x.BloodGroupId != dto.BloodGroupID &&
                    x.CreatedBy == dto.UserID &&
                    x.CompanyId == dto.CompanyID &&
                    x.IsDeleted == false &&
                    x.BloodGroupName.ToUpper().Trim() == normalizedName
                );

                if (duplicate)
                    return new ApiResponse<BloodGroupDto>(null, "Blood group already exists", false);

                // ✅ Update
                entity.BloodGroupName = normalizedName;
                entity.Description = dto.Description ?? "";
                entity.CompanyId = dto.CompanyID;
                entity.RegionId = dto.RegionID;
                entity.IsActive = dto.IsActive;
                entity.ModifiedBy = dto.UserID;
                entity.ModifiedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return new ApiResponse<BloodGroupDto>(
                    MapToDto(entity),
                    "Blood group updated successfully",
                    true
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodGroupDto>(null, ex.Message, false);
            }
        }

        #endregion


        #region Delete

        public async Task<ApiResponse<bool>> DeleteBloodGroupAsync(int id)
        {
            try
            {
                var entity = await _context.BloodGroups
                    .FirstOrDefaultAsync(x => x.BloodGroupId == id);

                if (entity == null)
                    return new ApiResponse<bool>(false, "Record not found", false);

                // ✅ Permanent delete
                _context.BloodGroups.Remove(entity);

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>(true, "Deleted permanently", true);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, ex.Message, false);
            }
        }


        #endregion


        private BloodGroupDto MapToDto(BloodGroup bg)
        {
            return new BloodGroupDto
            {
                BloodGroupID = bg.BloodGroupId,
                BloodGroupName = bg.BloodGroupName,
                CompanyID = bg.CompanyId,
                RegionID = bg.RegionId,
                IsActive = bg.IsActive
            };
        }

    }
}
