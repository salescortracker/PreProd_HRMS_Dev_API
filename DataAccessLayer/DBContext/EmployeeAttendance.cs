using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class EmployeeAttendance
{
    public int AttendanceId { get; set; }

    public int? RegionId { get; set; }

    public int? CompanyId { get; set; }

    public string? EmployeeCode { get; set; }

    public string? EmployeeName { get; set; }

    public DateOnly? AttendanceDate { get; set; }

    public string? Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public TimeOnly? ClockInTime { get; set; }

    public TimeOnly? ClockOutTime { get; set; }

    public string? GrossTime { get; set; }
}
