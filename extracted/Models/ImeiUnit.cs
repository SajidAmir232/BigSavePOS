namespace POSApp.Data.Models;

public class ImeiUnit
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid ProductGuid { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Available"; // Available / Sold / Returned / Defective / InRepair
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    public DateTime? SoldDate { get; set; }
    public Guid? InvoiceGuid { get; set; }
    public Guid? CustomerGuid { get; set; }

    // Sync fields
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
