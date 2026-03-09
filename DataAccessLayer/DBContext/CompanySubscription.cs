using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class CompanySubscription
{
    public int SubscriptionId { get; set; }

    public int CompanyId { get; set; }

    public int PlanId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? PaymentStatus { get; set; }

    public decimal? AmountPaid { get; set; }

    public bool? IsActive { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;
}
