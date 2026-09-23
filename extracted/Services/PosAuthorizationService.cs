using POSApp.Data.Models;

namespace POSApp.Data.Services;

/// Server-side authorization for POS operations. UI visibility is never treated as security.
public sealed class PosAuthorizationService
{
    public static bool IsAdmin(User? user) => string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public static bool IsManager(User? user) => IsAdmin(user) || string.Equals(user?.Role, "Manager", StringComparison.OrdinalIgnoreCase);
    public static bool CanManageProducts(User? user) => IsAdmin(user) || IsManager(user);
    public static bool CanDeleteProducts(User? user) => IsAdmin(user);
    public static bool CanManagePurchases(User? user) => IsAdmin(user) || IsManager(user);
    public static bool CanAdjustInventory(User? user) => IsAdmin(user) || IsManager(user);
    public static bool CanManageUsers(User? user) => IsAdmin(user);
    public static bool CanManageBranches(User? user) => IsAdmin(user);
    public static bool CanViewSensitiveFinancials(User? user) => IsManager(user);
    public static bool CanExport(User? user) => user != null;
    public static bool CanApproveReturns(User? user) => IsManager(user);

    public static void Require(bool allowed, string message = "You are not authorized for this operation.")
    {
        if (!allowed) throw new UnauthorizedAccessException(message);
    }
}
