using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public sealed class BranchService
{
    public List<Branch> GetAll() { using var db = new LocalDbContext(); return db.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToList(); }
    public Branch Save(Branch branch, User actor)
    {
        PosAuthorizationService.Require(PosAuthorizationService.CanManageBranches(actor));
        using var db = new LocalDbContext();
        if (branch.Id == 0) db.Branches.Add(branch); else db.Branches.Update(branch);
        db.SaveChanges(); return branch;
    }
    public void AssignUser(int userId, int branchId, User actor, bool primary = false)
    {
        PosAuthorizationService.Require(PosAuthorizationService.CanManageBranches(actor));
        using var db = new LocalDbContext();
        if (!db.UserBranches.Any(x => x.UserId == userId && x.BranchId == branchId)) db.UserBranches.Add(new UserBranch { UserId = userId, BranchId = branchId, IsPrimary = primary });
        if (primary) { var others = db.UserBranches.Where(x => x.UserId == userId && x.BranchId != branchId); foreach (var o in others) o.IsPrimary = false; var u = db.Users.Find(userId); if (u != null) u.PrimaryBranchId = branchId; }
        db.SaveChanges();
    }
    public List<int> GetUserBranchIds(int userId) { using var db = new LocalDbContext(); return db.UserBranches.Where(x => x.UserId == userId).Select(x => x.BranchId).ToList(); }
}
