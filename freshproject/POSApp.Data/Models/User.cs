namespace POSApp.Data.Models;

public class User
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Cashier";
    public int? PrimaryBranchId { get; set; } // Admin / Manager / Cashier
    public bool IsActive { get; set; } = true;
    public int FailedAttemptCount { get; set; } = 0;
    public DateTime? LastFailedAttempt { get; set; }
    public bool ForcePasswordChange { get; set; } = false;

    // Subscription fields
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? AssignedBy { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Notes { get; set; }

    // Sync fields
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public bool IsSynced { get; set; } = false;

    // Computed
    public bool IsSubscriptionActive => SubscriptionEndDate == null || SubscriptionEndDate > DateTime.UtcNow;
}
