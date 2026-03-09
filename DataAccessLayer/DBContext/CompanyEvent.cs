using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class CompanyEvent
{
    public int Id { get; set; }

    public int? CompanyId { get; set; }

    public int? RegionId { get; set; }

    public int? DepartmentId { get; set; }

    public string? EventTitle { get; set; }

    public string? EventDescription { get; set; }

    public DateOnly? EventDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? MeetingLink { get; set; }

    public string? EventLocation { get; set; }

    public string? EventType { get; set; }

    public bool? IsMeeting { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UserId { get; set; }
}
