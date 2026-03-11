using BusinessLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<EmployeeAttendanceDto>> GetTodayEmployees(int companyId, int regionId);

        Task SaveAttendanceAsync(SaveAttendanceDto dto, int userId);

        Task<List<EmployeeAttendanceDto>> GetWeeklyReport(int companyId, int regionId);

        Task<List<EmployeeAttendanceDto>> GetMonthlyReport(int companyId, int regionId);
        Task<List<EmployeeAttendanceDto>> GetDateRangeReport(
           int companyId,
           int regionId,
           DateTime fromDate,
           DateTime toDate);
    }
}
