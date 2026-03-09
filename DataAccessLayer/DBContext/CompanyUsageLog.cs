using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class CompanyUsageLog
{
    public int UsageId { get; set; }

    public int CompanyId { get; set; }

    public int? ActiveUsers { get; set; }

    public int? TotalLogins { get; set; }

    public decimal? StorageUsedMb { get; set; }

    public DateTime? LoggedDate { get; set; }
}
