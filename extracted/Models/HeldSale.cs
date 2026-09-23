namespace POSApp.Data.Models;

public class HeldSale
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public int BranchId { get; set; }
    public Guid? CustomerGuid { get; set; }
    public string Label { get; set; } = "Held Sale";
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public DateTime HeldAtUtc { get; set; } = DateTime.UtcNow;
    public string? HeldBy { get; set; }
    public List<HeldSaleItem> Items { get; set; } = new();
}

public class HeldSaleItem
{
    public int Id { get; set; }
    public Guid HeldSaleGuid { get; set; }
    public Guid ProductGuid { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
