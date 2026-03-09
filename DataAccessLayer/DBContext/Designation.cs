using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class Designation
{
    public int DesignationId { get; set; }

    public int CompanyId { get; set; }

    public int RegionId { get; set; }

    public string DesignationName { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? UserId { get; set; }

    public int? DepartmentId { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Department? Department { get; set; }

    public virtual Region Region { get; set; } = null!;
}
