namespace POSApp.Data.Models;

public class JournalEntry
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid DebitAccountGuid { get; set; }
    public Guid CreditAccountGuid { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.Now;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;
}
