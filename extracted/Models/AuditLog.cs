namespace POSApp.Data.Models;

public class AuditLog
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
