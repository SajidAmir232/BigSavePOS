namespace POSApp.Data.Models;

public class Product
{
    public int Id { get; set; }                     // Local auto id
    public Guid Guid { get; set; } = Guid.NewGuid(); // Global id used for sync (avoids collisions between offline devices)
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Brand { get; set; }
    public decimal WholesalePrice { get; set; }
    public int MinimumStock { get; set; } = 10;
    public DateTime? ExpiryDate { get; set; }
    public string? ImageUrl { get; set; }
    public int BranchId { get; set; } = 1;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = "pcs";
    public bool IsActive { get; set; } = true;
    public string? IMEI { get; set; } // Mobile products only (15-digit format)

    // --- Sync bookkeeping fields (every syncable table needs these) ---
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;      // false = still needs to be pushed to server
}
