using System;
using System.Collections.Generic;

namespace DataAccessLayer.DBContext;

public partial class AuditLog
{
    public long AuditId { get; set; }

    public int? CompanyId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public string? OldData { get; set; }

    public string? NewData { get; set; }

    public DateTime? CreatedAt { get; set; }
}
