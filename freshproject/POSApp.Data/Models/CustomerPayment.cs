namespace POSApp.Data.Models;

public class CustomerPayment
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid CustomerGuid { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public string? Note { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
