using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using DataAccessLayer.Repositories.GeneralRepository;

namespace BusinessLayer.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================================
        // GET TODAY EMPLOYEES
        // ================================
        public async Task<List<EmployeeAttendanceDto>> GetTodayEmployees(int companyId, int regionId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
          

            var users = (await _unitOfWork.Repository<User>().GetAllAsync())
                .Where(e => e.CompanyId == companyId
                         && e.RegionId == regionId
                         && !string.IsNullOrEmpty(e.EmployeeCode))
                .ToList();

            var clockRecords = await _unitOfWork.Repository<ClockInOut>().GetAllAsync();
            var leaves = await _unitOfWork.Repository<LeaveRequest>().GetAllAsync();
            var leaveTypes = await _unitOfWork.Repository<LeaveType>().GetAllAsync();

            var result = new List<EmployeeAttendanceDto>();

            foreach (var emp in users)
            {
                string status = "Absent";
                string clockInTime = null;
                string clockOutTime = null;
                string grossTime = null;

                // ================= LEAVE CHECK =================
                var leave = leaves.FirstOrDefault(l =>
                    l.UserId == emp.UserId &&
                    l.Status == "Approved" &&
                    l.StartDate <= today &&
                    l.EndDate >= today);

                if (leave != null)
                {
                    var leaveType = leaveTypes
                        .FirstOrDefault(t => t.LeaveTypeId == leave.LeaveTypeId);

                    status = leaveType?.LeaveTypeName ?? "Leave";
                }
                else
                {
                    var records = clockRecords
                        .Where(c =>
                            c.EmployeeCode == emp.EmployeeCode &&
                            c.CompanyId == companyId &&
                            c.RegionId == regionId &&
                            c.AttendanceDate == today)
                        .OrderBy(c => c.ActionTime)
                        .ToList();

                    var clockIn = records
                       .FirstOrDefault(r => r.ActionType == "ClockIn")?.ActionTime;

                    var clockOut = records
                        .LastOrDefault(r => r.ActionType == "ClockOut")?.ActionTime;

                    if (clockIn != null)
                        clockInTime = clockIn.Value.ToString("HH:mm");

                    if (clockOut != null)
                        clockOutTime = clockOut.Value.ToString("HH:mm");

                    if (clockIn != null && clockOut != null)
                    {
                        var duration = clockOut.Value - clockIn.Value;

                        grossTime = duration.ToString(@"hh\:mm");

                        if (duration.TotalHours >= 7)
                            status = "Present";
                        else
                            status = "HalfDay";
                    }
                }

                result.Add(new EmployeeAttendanceDto
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.FullName,
                    AttendanceDate = DateTime.Today,
                    Status = status,
                    ClockIn = clockInTime,
                    ClockOut = clockOutTime,
                    GrossTime = grossTime
                });
            }

            return result;
        }
        // ================================
        // SAVE ATTENDANCE
        // ================================
        public async Task SaveAttendanceAsync(SaveAttendanceDto dto, int userId)
        {
            var repo = _unitOfWork.Repository<EmployeeAttendance>();

            var attendanceDate = DateOnly.FromDateTime(dto.AttendanceDate);

            // Get existing attendance records for that date
            var existingRecords = (await repo.GetAllAsync())
                .Where(x => x.CompanyId == dto.CompanyId &&
                            x.RegionId == dto.RegionId &&
                            x.AttendanceDate == attendanceDate)
                .ToList();

            foreach (var emp in dto.Employees)
            {
                var existing = existingRecords
                    .FirstOrDefault(x => x.EmployeeCode == emp.EmployeeCode);

                if (existing != null)
                {
                    // ================= UPDATE EXISTING =================
                    existing.Status = emp.Status;

                    existing.ClockInTime = string.IsNullOrEmpty(emp.ClockIn)
                        ? null
                        : TimeOnly.Parse(emp.ClockIn);

                    existing.ClockOutTime = string.IsNullOrEmpty(emp.ClockOut)
                        ? null
                        : TimeOnly.Parse(emp.ClockOut);

                    existing.GrossTime = emp.GrossTime;

                    existing.ModifiedBy = userId.ToString();
                    existing.ModifiedAt = DateTime.Now;
                }
                else
                {
                    // ================= INSERT NEW =================
                    var entity = new EmployeeAttendance
                    {
                        RegionId = dto.RegionId,
                        CompanyId = dto.CompanyId,
                        EmployeeCode = emp.EmployeeCode,
                        EmployeeName = emp.EmployeeName,
                        AttendanceDate = attendanceDate,
                        Status = emp.Status,

                        ClockInTime = string.IsNullOrEmpty(emp.ClockIn)
                            ? null
                            : TimeOnly.Parse(emp.ClockIn),

                        ClockOutTime = string.IsNullOrEmpty(emp.ClockOut)
                            ? null
                            : TimeOnly.Parse(emp.ClockOut),

                        GrossTime = emp.GrossTime,

                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    };

                    await repo.AddAsync(entity);
                }
            }

            await _unitOfWork.CompleteAsync();
        }

        // ================================
        // DATES RANGE REPORT
        // ================================
        public async Task<List<EmployeeAttendanceDto>> GetDateRangeReport(
    int companyId,
    int regionId,
    DateTime fromDate,
    DateTime toDate)
        {
            var data = await _unitOfWork.Repository<EmployeeAttendance>().GetAllAsync();

            var startDate = DateOnly.FromDateTime(fromDate);
            var endDate = DateOnly.FromDateTime(toDate);

            return data
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.RegionId == regionId &&
                    x.AttendanceDate.HasValue &&
                    x.AttendanceDate.Value >= startDate &&
                    x.AttendanceDate.Value <= endDate)
                .OrderByDescending(x => x.AttendanceDate)
                .Select(MapToDto)
                .ToList();
        }

        // ================================
        // WEEKLY REPORT
        // ================================
        public async Task<List<EmployeeAttendanceDto>> GetWeeklyReport(int companyId, int regionId)
        {
            var data = await _unitOfWork.Repository<EmployeeAttendance>().GetAllAsync();

            var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
            var endDate = DateOnly.FromDateTime(DateTime.Today);

            return data
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.RegionId == regionId &&
                    x.AttendanceDate.HasValue &&
                    x.AttendanceDate.Value >= startDate &&
                    x.AttendanceDate.Value <= endDate)
                .OrderByDescending(x => x.AttendanceDate)
                .Select(MapToDto)
                .ToList();
        }

        // ================================
        // MONTHLY REPORT
        // ================================
        public async Task<List<EmployeeAttendanceDto>> GetMonthlyReport(int companyId, int regionId)
        {
            var data = await _unitOfWork.Repository<EmployeeAttendance>().GetAllAsync();

            var today = DateTime.Today;

            return data
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.RegionId == regionId &&
                    x.AttendanceDate.HasValue &&
                    x.AttendanceDate.Value.Month == today.Month &&
                    x.AttendanceDate.Value.Year == today.Year)
                .OrderByDescending(x => x.AttendanceDate)
                .Select(MapToDto)
                .ToList();
        }

        // ================================
        // MAP ENTITY → DTO
        // ================================
        private static EmployeeAttendanceDto MapToDto(EmployeeAttendance entity)
        {
            return new EmployeeAttendanceDto
            {
                AttendanceId = entity.AttendanceId,

                RegionId = entity.RegionId ?? 0,
                CompanyId = entity.CompanyId ?? 0,

                EmployeeCode = entity.EmployeeCode ?? "",
                EmployeeName = entity.EmployeeName ?? "",

                AttendanceDate = entity.AttendanceDate.HasValue
                    ? entity.AttendanceDate.Value.ToDateTime(TimeOnly.MinValue)
                    : DateTime.MinValue,

                Status = entity.Status ?? "",

                ClockIn = entity.ClockInTime?.ToString("HH:mm"),
                ClockOut = entity.ClockOutTime?.ToString("HH:mm"),
                GrossTime = entity.GrossTime
            };
        }
    }
}