namespace POSApp.Data.Models;

public class RepairJob
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string JobNumber { get; set; } = string.Empty;
    public Guid CustomerGuid { get; set; }
    public string DeviceModel { get; set; } = string.Empty;
    public string? IMEI { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending / InProgress / Completed / Delivered
    public DateTime DateReceived { get; set; } = DateTime.Now;
    public DateTime? DateDelivered { get; set; }
    public string? Notes { get; set; }

    public List<RepairJobPart> Parts { get; set; } = new();

    // Sync fields
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}

public class RepairJobPart
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid RepairJobGuid { get; set; }
    public Guid ProductGuid { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }

    // Sync fields
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
