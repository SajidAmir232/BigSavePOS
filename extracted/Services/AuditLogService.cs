using POSApp.Data.Models;

namespace POSApp.Data.Services;

public sealed class AuditLogService
{
    public void Log(User? user, string action, string entity, string? entityId = null, string? details = null, int? branchId = null)
    {
        using var db = new LocalDbContext();
        db.AuditLogs.Add(new AuditLog { UserId = user?.Id, BranchId = branchId ?? user?.PrimaryBranchId, Action = action, Entity = entity, EntityId = entityId, Details = details });
        db.SaveChanges();
    }

    public List<AuditLog> GetRecent(int take = 200)
    {
        using var db = new LocalDbContext();
        return db.AuditLogs.OrderByDescending(x => x.CreatedAtUtc).Take(Math.Clamp(take, 1, 1000)).ToList();
    }
}
