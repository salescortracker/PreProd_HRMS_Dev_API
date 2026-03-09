using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class CompanyRegion
{
    public int RegionId { get; set; }

    public int CompanyId { get; set; }

    public string RegionName { get; set; } = null!;

    public string RegionCode { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual SuperadminCompany Company { get; set; } = null!;
}
