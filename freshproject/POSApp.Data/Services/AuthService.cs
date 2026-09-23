using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class AuthService
{
    private const int MaxFailedAttempts = 5;
    private readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(5);

    public (User? user, string message) Login(string username, string password)
    {
        username = username?.Trim() ?? string.Empty;
        password ??= string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return (null, "Please enter username/email and password.");

        try
        {
            using var db = new LocalDbContext();

            // Support both username and email sign-in.
            var user = db.Users.FirstOrDefault(u =>
                !u.IsDeleted &&
                (u.Username.ToLower() == username.ToLower() || u.Email.ToLower() == username.ToLower()));

            if (user == null)
                return (null, "Invalid username/email or password.");

            if (!user.IsActive)
                return (null, "Your account has been deactivated. Contact admin.");

            if (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value < DateTime.UtcNow)
                return (null, "Your subscription has expired. Contact admin to renew.");

            if (user.FailedAttemptCount >= MaxFailedAttempts && user.LastFailedAttempt.HasValue)
            {
                var lockoutEnd = user.LastFailedAttempt.Value.Add(_lockoutDuration);
                if (DateTime.UtcNow < lockoutEnd)
                {
                    var remainingMinutes = (int)Math.Ceiling((lockoutEnd - DateTime.UtcNow).TotalMinutes);
                    return (null, $"Too many failed attempts. Try again in {remainingMinutes} minute(s).");
                }

                user.FailedAttemptCount = 0;
                user.LastFailedAttempt = null;
            }

            bool passwordValid;
            try
            {
                passwordValid = !string.IsNullOrWhiteSpace(user.PasswordHash) &&
                                BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception)
            {
                passwordValid = false;
            }

            if (!passwordValid)
            {
                user.FailedAttemptCount++;
                user.LastFailedAttempt = DateTime.UtcNow;
                user.IsSynced = false;
                user.UpdatedAtUtc = DateTime.UtcNow;
                db.SaveChanges();
                return (null, "Invalid username/email or password.");
            }

            user.FailedAttemptCount = 0;
            user.LastFailedAttempt = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.IsSynced = false;
            user.UpdatedAtUtc = DateTime.UtcNow;
            db.SaveChanges();

            return (new User
            {
                Id = user.Id, Guid = user.Guid, Username = user.Username, Email = user.Email,
                PasswordHash = user.PasswordHash, Role = user.Role, IsActive = user.IsActive,
                IsDeleted = user.IsDeleted, IsSynced = user.IsSynced, UpdatedAtUtc = user.UpdatedAtUtc,
                FailedAttemptCount = user.FailedAttemptCount, LastFailedAttempt = user.LastFailedAttempt,
                ForcePasswordChange = user.ForcePasswordChange, SubscriptionStartDate = user.SubscriptionStartDate,
                SubscriptionEndDate = user.SubscriptionEndDate, AssignedBy = user.AssignedBy,
                LastLoginAt = user.LastLoginAt, Notes = user.Notes, PrimaryBranchId = user.PrimaryBranchId
            }, "");
        }
        catch (Exception ex)
        {
            // Never expose stack traces to the user, but provide a useful actionable message.
            return (null, $"Sign-in could not be completed. Please check the database/configuration. ({ex.Message})");
        }
    }

    public bool IsUsernameTaken(string username)
    {
        using var db = new LocalDbContext();
        return db.Users.Any(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
    }

    public bool IsEmailTaken(string email)
    {
        using var db = new LocalDbContext();
        return db.Users.Any(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }

    public (bool success, string message) SignUp(string username, string email, string password, string confirmPassword)
    {
        username = username?.Trim() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return (false, "Username must be at least 3 characters.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            return (false, "Username can only contain letters, numbers, and underscore.");

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return (false, "Please enter a valid email address.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters.");

        if (password != confirmPassword)
            return (false, "Passwords do not match.");

        using var db = new LocalDbContext();

        if (db.Users.Any(u => u.Username.ToLower() == username.ToLower()))
            return (false, "Username already exists.");

        if (db.Users.Any(u => u.Email.ToLower() == email.ToLower()))
            return (false, "Email already registered.");

        try
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Cashier",
                IsActive = true,
                IsSynced = false,
                UpdatedAtUtc = DateTime.UtcNow,
                FailedAttemptCount = 0,
                ForcePasswordChange = false
            };
            db.Users.Add(user);
            db.SaveChanges();
            return (true, "Account created successfully. You can now login.");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating account: {ex.Message}");
        }
    }

    public (bool success, string message) AdminResetPassword(int userId, string newTemporaryPassword, User? actor = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor), "Only Admin can reset another user password.");
        if (string.IsNullOrWhiteSpace(newTemporaryPassword) || newTemporaryPassword.Length < 6)
            return (false, "Password must be at least 6 characters.");

        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
            return (false, "User not found.");

        try
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newTemporaryPassword);
            user.ForcePasswordChange = true;
            user.FailedAttemptCount = 0;
            user.LastFailedAttempt = null;
            user.IsSynced = false;
            user.UpdatedAtUtc = DateTime.UtcNow;
            db.SaveChanges();
            return (true, "Password reset successfully. User must change password on next login.");
        }
        catch (Exception ex)
        {
            return (false, $"Error resetting password: {ex.Message}");
        }
    }

    public (bool success, string message) ChangePassword(int userId, string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "New password must be at least 6 characters.");

        if (newPassword != confirmPassword)
            return (false, "Passwords do not match.");

        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
            return (false, "User not found.");

        bool passwordValid = BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash);
        if (!passwordValid)
            return (false, "Current password is incorrect.");

        try
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ForcePasswordChange = false;
            user.IsSynced = false;
            user.UpdatedAtUtc = DateTime.UtcNow;
            db.SaveChanges();
            return (true, "Password changed successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error changing password: {ex.Message}");
        }
    }

    public (bool success, string message) RequestPasswordReset(string email)
    {
        return (true, "Please contact your system administrator to reset your password.");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public User CreateUser(string username, string password, string role)
    {
        using var db = new LocalDbContext();
        var user = new User
        {
            Username = username,
            Email = $"{username}@posapp.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            IsSynced = false,
            UpdatedAtUtc = DateTime.UtcNow,
            FailedAttemptCount = 0,
            ForcePasswordChange = false
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public List<User> GetAllUsers()
    {
        using var db = new LocalDbContext();
        return db.Users.Where(u => !u.IsDeleted).OrderBy(u => u.Username).ToList();
    }

    public User? GetUserById(int id)
    {
        using var db = new LocalDbContext();
        return db.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
    }

    public (bool success, string message) AdminCreateUser(string username, string email, string password, string role, string assignedBy, int durationMonths, User? actor = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor), "Only Admin can create users.");
        var (ok, msg) = SignUp(username, email, password, password);
        if (!ok) return (false, msg);

        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        if (user != null)
        {
            user.Role = role;
            user.AssignedBy = assignedBy;
            user.SubscriptionStartDate = DateTime.UtcNow;
            user.SubscriptionEndDate = DateTime.UtcNow.AddMonths(durationMonths);
            db.SaveChanges();
        }
        return (true, $"User '{username}' created successfully with {durationMonths} month(s) subscription.");
    }

    public (bool success, string message) UpdateUserRole(int userId, string role, User? actor = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor), "Only Admin can change user roles.");
        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId && !u.IsDeleted);
        if (user == null) return (false, "User not found.");
        user.Role = role;
        user.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
        return (true, "Role updated successfully.");
    }

    public (bool success, string message) UpdateSubscription(int userId, int durationMonths, User? actor = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor), "Only Admin can update subscriptions.");
        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId && !u.IsDeleted);
        if (user == null) return (false, "User not found.");
        user.SubscriptionStartDate = DateTime.UtcNow;
        user.SubscriptionEndDate = DateTime.UtcNow.AddMonths(durationMonths);
        user.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
        return (true, $"Subscription extended by {durationMonths} month(s).");
    }

    public (bool success, string message) ToggleUserActive(int userId, User? actor = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor), "Only Admin can activate/deactivate users.");
        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId && !u.IsDeleted);
        if (user == null) return (false, "User not found.");
        user.IsActive = !user.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
        return (true, user.IsActive ? "User activated." : "User deactivated.");
    }

    public (bool success, string message) DeleteUser(int userId)
    {
        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId && !u.IsDeleted);
        if (user == null) return (false, "User not found.");
        if (user.Role == "Admin") return (false, "Cannot delete admin user.");
        user.IsDeleted = true;
        user.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
        return (true, "User deleted.");
    }

    public (bool success, string message) AdminResetPasswordByEmail(string email, string newPassword)
    {
        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        if (user == null) return (false, "No account found with this email.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ForcePasswordChange = true;
        user.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
        return (true, $"Password reset. Temporary password: {newPassword}");
    }
}
