namespace POSApp.Data.Models;

public class Expense
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; } = DateTime.Now;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
