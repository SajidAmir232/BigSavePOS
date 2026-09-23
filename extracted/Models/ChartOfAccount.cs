namespace POSApp.Data.Models;

public class ChartOfAccount
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // Cash/Bank/Sales/Purchases/Expenses/Receivables/Payables

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
