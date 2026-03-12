using BusinessLayer.Common;
using BusinessLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IHolidayListService
    {
        Task<ApiResponse<IEnumerable<HolidayListDto>>> GetAll(int userId);
        Task<ApiResponse<IEnumerable<HolidayListDto>>> GetAllInCalender(int companyId, int regionId);
        Task<ApiResponse<HolidayListDto?>> GetByIdAsync(int id);

        Task<ApiResponse<string>> CreateAsync(CreateUpdateHolidayListDto dto);

        Task<ApiResponse<string>> UpdateAsync(CreateUpdateHolidayListDto dto);

        Task<ApiResponse<string>> DeleteAsync(int id);

        Task<ApiResponse<IEnumerable<HolidayListDto>>> Getholidaybycompanyidandregionid(int CompanyID, int RegionId);

        Task<ApiResponse<IEnumerable<EventDTO>>> Geteventsbycompanyidandregionid(int CompanyID, int RegionId);
        //Task<ApiResponse<IEnumerable<EmployeeDto>>> GetBirthdaysbycompanyidandregionid(int CompanyID, int RegionId);
        Task<ApiResponse<IEnumerable<LeaveRequestDto>>> GetApprovedLeavesbyUserid(int userId);

        Task<ApiResponse<IEnumerable<PersonalDetailsDto>>>
            GetBirthdaysByCompanyAndRegion(int companyId, int regionId);

    }
}
