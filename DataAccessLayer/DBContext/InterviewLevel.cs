using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class InterviewLevel
{
    public int InterviewLevelsId { get; set; }

    public int? UserId { get; set; }

    public int CompanyId { get; set; }

    public int RegionId { get; set; }

    public string? InterviewLevels { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<CandidateInterview> CandidateInterviews { get; set; } = new List<CandidateInterview>();

    public virtual Company Company { get; set; } = null!;

    public virtual Region Region { get; set; } = null!;
}
