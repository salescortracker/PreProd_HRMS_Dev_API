using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class SubscriptionPlan
{
    public int PlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public int MaxUsers { get; set; }

    public int MaxRegions { get; set; }

    public decimal Price { get; set; }

    public int DurationInDays { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<CompanySubscription> CompanySubscriptions { get; set; } = new List<CompanySubscription>();
}
