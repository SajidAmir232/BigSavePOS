namespace POSApp.Data.Models;

public class SalePayment
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid InvoiceGuid { get; set; }
    public string Method { get; set; } = "Cash"; // Cash, Card, Bank Transfer
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
}
