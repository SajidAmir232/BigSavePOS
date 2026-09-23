namespace POSApp.Data.Models;

public class UserBranch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
}
