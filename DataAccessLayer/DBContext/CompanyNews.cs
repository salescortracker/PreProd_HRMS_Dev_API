using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class CompanyNews
{
    public int NewsId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateOnly? PostedDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public int? UserId { get; set; }

    public int? CompanyId { get; set; }

    public int? RegionId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
