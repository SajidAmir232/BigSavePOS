namespace POSApp.Data.Models;

public class Customer
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; } // Total credit owed
    public decimal CreditLimit { get; set; }

    // Sync fields
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
