using BusinessLayer.Common;
using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using DataAccessLayer.Repositories.GeneralRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Implementations
{
    public class HolidayListService: IHolidayListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HRMSContext _context;
        public HolidayListService(IUnitOfWork unitOfWork, HRMSContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // GET ALL
        public async Task<ApiResponse<IEnumerable<HolidayListDto>>> GetAll(int userId)
        {
            var list = (await _unitOfWork.Repository<HolidayList>()
                .FindAsync(x => !x.IsDeleted && x.UserId == userId))
                .OrderByDescending(x => x.HolidayListId)
                .ToList();

            var dto = list.Select(x => new HolidayListDto
            {
                HolidayListID = x.HolidayListId,
                CompanyID = x.CompanyId,
                RegionID = x.RegionId,
                HolidayListName = x.HolidayListName,
                Date = x.Date,
                IsActive = x.IsActive
            });

            return new ApiResponse<IEnumerable<HolidayListDto>>(dto, "Holiday List retrieved successfully.");
        }

        // GET BY ID
        public async Task<ApiResponse<HolidayListDto?>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<HolidayList>().GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return new ApiResponse<HolidayListDto?>(null, "Holiday not found.", false);

            var dto = new HolidayListDto
            {
                HolidayListID = entity.HolidayListId,
                CompanyID = entity.CompanyId,
                RegionID = entity.RegionId,
                HolidayListName = entity.HolidayListName,
                Date = entity.Date,
                IsActive = entity.IsActive
            };

            return new ApiResponse<HolidayListDto?>(dto, "Holiday retrieved successfully.");
        }

        // CREATE
        public async Task<ApiResponse<string>> CreateAsync(CreateUpdateHolidayListDto dto)
        {
            var duplicate = (await _unitOfWork.Repository<HolidayList>().FindAsync(x =>
                !x.IsDeleted &&
                x.CompanyId == dto.CompanyID &&
                x.RegionId == dto.RegionID &&
                x.HolidayListName.ToLower() == dto.HolidayListName.ToLower()))
                .Any();

            if (duplicate)
                return new ApiResponse<string>(null!, "Duplicate Holiday exists.", false);

            var entity = new HolidayList
            {
                CompanyId = dto.CompanyID,
                RegionId = dto.RegionID,
                HolidayListName = dto.HolidayListName,
                Date = dto.Date,
                IsActive = dto.IsActive,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UserId = dto.UserId,
                CreatedBy = dto.UserId ?? 0
            };

            await _unitOfWork.Repository<HolidayList>().AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<string>("Holiday created successfully.");
        }

        // UPDATE
        public async Task<ApiResponse<string>> UpdateAsync(CreateUpdateHolidayListDto dto)
        {
            var entity = await _unitOfWork.Repository<HolidayList>()
                .GetByIdAsync(dto.HolidayListID);

            if (entity == null || entity.IsDeleted)
                return new ApiResponse<string>(null!, "Holiday not found.", false);

            entity.CompanyId = dto.CompanyID;
            entity.RegionId = dto.RegionID;

            entity.HolidayListName = dto.HolidayListName;
            entity.Date = dto.Date;
            entity.IsActive = dto.IsActive;
            entity.ModifiedAt = DateTime.UtcNow;
            entity.ModifiedBy = dto.UserId;

            _unitOfWork.Repository<HolidayList>().Update(entity);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<string>("Holiday updated successfully.");
        }

        // DELETE (SOFT)
        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<HolidayList>().GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return new ApiResponse<string>(null!, "Holiday not found.", false);

            entity.IsDeleted = true;
            entity.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.Repository<HolidayList>().Update(entity);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<string>("Holiday deleted successfully.");
        }
        // get all by companyid and regionid
      



        public async Task<ApiResponse<IEnumerable<HolidayListDto>>> Getholidaybycompanyidandregionid(int CompanyID, int RegionId)
        {
            var list = (await _unitOfWork.Repository<HolidayList>()
                .FindAsync(x => !x.IsDeleted && x.CompanyId == CompanyID && x.RegionId == RegionId))
                .OrderByDescending(x => x.HolidayListId)
                .ToList();

            var dto = list.Select(x => new HolidayListDto
            {
                HolidayListID = x.HolidayListId,
                CompanyID = x.CompanyId,
                RegionID = x.RegionId,
                HolidayListName = x.HolidayListName,
                Date = x.Date,
                IsActive = x.IsActive
            });

            return new ApiResponse<IEnumerable<HolidayListDto>>(dto, "Holiday List retrieved successfully.");
        }
        public async Task<ApiResponse<IEnumerable<EventDTO>>> Geteventsbycompanyidandregionid(int CompanyID, int RegionId)
        {
            var list = (await _unitOfWork.Repository<Event>()
                .FindAsync(x => x.CompanyId == CompanyID && x.RegionId == RegionId))
                .OrderByDescending(x => x.EventId)
                .ToList();

            var dto = list.Select(x => new EventDTO
            {
                EventId = x.EventId,
                CompanyId = x.CompanyId,
                RegionId = x.RegionId,
                EventName = x.EventName,
                EventDate = x.EventDate,
                IsActive = x.IsActive
            });

            return new ApiResponse<IEnumerable<EventDTO>>(dto, "Events retrieved successfully.");
        }
        public async Task<ApiResponse<IEnumerable<LeaveRequestDto>>>
      GetApprovedLeavesbyUserid(int userId)
        {
            var list = (await _unitOfWork.Repository<LeaveRequest>()
                .FindAsync(x => x.UserId == userId
                             && x.Status == "Approved"))
                .OrderByDescending(x => x.LeaveRequestId)
                .ToList();

            var dto = list.Select(x => new LeaveRequestDto
            {
                LeaveRequestId = x.LeaveRequestId,
                CompanyId = x.CompanyId,
                RegionId = x.RegionId,
                //LeaveTypeName = ,
                AppliedDate = x.AppliedDate,
            });

            return new ApiResponse<IEnumerable<LeaveRequestDto>>(
                dto,
                "Approved leaves retrieved successfully."
            );
        }

        public async Task<ApiResponse<IEnumerable<PersonalDetailsDto>>>
       GetBirthdaysByCompanyAndRegion(int companyId, int regionId)
        {
            var today = DateTime.Today;

            var result = _context.EmployeePersonalDetails

                .Where(x =>
                    x.CompanyId == companyId &&
                    x.RegionId == regionId)
                .Select(x => new PersonalDetailsDto
                {
                    userId = x.UserId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DateOfBirth = x.DateOfBirth
                });

            return new ApiResponse<IEnumerable<PersonalDetailsDto>>(
                result,
                "Today's birthdays retrieved successfully."
            );
        }
    }
}
