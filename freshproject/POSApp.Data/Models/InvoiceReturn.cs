namespace POSApp.Data.Models;

public class InvoiceReturn
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid OriginalInvoiceGuid { get; set; }
    public Guid ProductGuid { get; set; }
    public string? IMEI { get; set; }
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.Now;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
