using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class SuperadminCompany
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string CompanyCode { get; set; } = null!;

    public string? IndustryType { get; set; }

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CompanyRegion> CompanyRegions { get; set; } = new List<CompanyRegion>();
}
