using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class RepairJobService
{
    private readonly ProductService _productService = new();

    public List<RepairJob> GetAll()
    {
        using var db = new LocalDbContext();
        return db.RepairJobs.Where(r => !r.IsDeleted).OrderByDescending(r => r.DateReceived).ToList();
    }

    public List<RepairJob> GetByStatus(string status)
    {
        using var db = new LocalDbContext();
        return db.RepairJobs
            .Where(r => r.Status == status && !r.IsDeleted)
            .OrderByDescending(r => r.DateReceived)
            .ToList();
    }

    public List<RepairJob> GetByCustomerGuid(Guid customerGuid)
    {
        using var db = new LocalDbContext();
        return db.RepairJobs
            .Where(r => r.CustomerGuid == customerGuid && !r.IsDeleted)
            .OrderByDescending(r => r.DateReceived)
            .ToList();
    }

    public RepairJob? GetByGuid(Guid guid)
    {
        using var db = new LocalDbContext();
        return db.RepairJobs
            .Include(r => r.Parts)
            .FirstOrDefault(r => r.Guid == guid && !r.IsDeleted);
    }

    public RepairJob Save(RepairJob repairJob)
    {
        using var db = new LocalDbContext();
        repairJob.UpdatedAtUtc = DateTime.UtcNow;
        repairJob.IsSynced = false;

        if (repairJob.Id == 0)
        {
            repairJob.JobNumber = GenerateJobNumber(db);
            db.RepairJobs.Add(repairJob);
            db.SaveChanges();

            foreach (var part in repairJob.Parts)
            {
                part.RepairJobGuid = repairJob.Guid;
                part.UpdatedAtUtc = DateTime.UtcNow;
                part.IsSynced = false;
                db.RepairJobParts.Add(part);
            }
        }
        else
        {
            db.RepairJobs.Update(repairJob);
        }

        db.SaveChanges();
        return repairJob;
    }

    public void UpdateStatus(Guid repairJobGuid, string newStatus)
    {
        using var db = new LocalDbContext();
        var job = db.RepairJobs.FirstOrDefault(r => r.Guid == repairJobGuid && !r.IsDeleted);
        if (job == null) throw new Exception("Repair job not found.");

        var validTransitions = new Dictionary<string, string[]>
        {
            ["Pending"] = new[] { "InProgress", "Cancelled" },
            ["InProgress"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = new[] { "Delivered" },
            ["Delivered"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

        if (!validTransitions.ContainsKey(job.Status) || !validTransitions[job.Status].Contains(newStatus))
        {
            throw new Exception($"Cannot transition from '{job.Status}' to '{newStatus}'.");
        }

        job.Status = newStatus;
        if (newStatus == "Delivered")
            job.DateDelivered = DateTime.Now;

        job.IsSynced = false;
        job.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    public void AddPart(RepairJobPart part)
    {
        using var db = new LocalDbContext();
        part.UpdatedAtUtc = DateTime.UtcNow;
        part.IsSynced = false;

        db.RepairJobParts.Add(part);

        // Deduct stock for the spare part
        _productService.AdjustStock(part.ProductGuid, -part.Quantity);

        db.SaveChanges();
    }

    public void RemovePart(Guid partGuid)
    {
        using var db = new LocalDbContext();
        var part = db.RepairJobParts.FirstOrDefault(p => p.Guid == partGuid && !p.IsDeleted);
        if (part == null) return;

        // Restore stock
        _productService.AdjustStock(part.ProductGuid, part.Quantity);

        part.IsDeleted = true;
        part.IsSynced = false;
        part.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    public List<RepairJobPart> GetPartsByRepairJobGuid(Guid repairJobGuid)
    {
        using var db = new LocalDbContext();
        return db.RepairJobParts
            .Where(p => p.RepairJobGuid == repairJobGuid && !p.IsDeleted)
            .ToList();
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var job = db.RepairJobs.Find(id);
        if (job == null) return;

        job.IsDeleted = true;
        job.IsSynced = false;
        job.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    private string GenerateJobNumber(LocalDbContext db)
    {
        var datePrefix = DateTime.Now.ToString("yyyyMMdd");
        var existingCount = db.RepairJobs
            .Where(r => r.JobNumber.StartsWith("RPR-" + datePrefix))
            .Count();
        var sequenceNumber = existingCount + 1;
        return $"RPR-{datePrefix}-{sequenceNumber:D4}";
    }
}
