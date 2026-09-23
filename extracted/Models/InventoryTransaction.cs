namespace POSApp.Data.Models;

public class InventoryTransaction
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid ProductGuid { get; set; }
    public int BranchId { get; set; }
    public decimal QuantityChange { get; set; }
    public int QuantityAfter { get; set; }
    public string Type { get; set; } = "Adjustment"; // Purchase, Sale, Return, Adjustment, Opening
    public string? Reference { get; set; }
    public string? Reason { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
