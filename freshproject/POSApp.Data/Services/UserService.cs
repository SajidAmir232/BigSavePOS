using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class UserService
{
    public User? GetByUsername(string username)
    {
        using var db = new LocalDbContext();
        return db.Users.FirstOrDefault(u => u.Username == username && !u.IsDeleted && u.IsActive);
    }

    public User? GetByEmail(string email)
    {
        using var db = new LocalDbContext();
        return db.Users.FirstOrDefault(u => u.Email == email && !u.IsDeleted && u.IsActive);
    }

    public bool UserExists(string email)
    {
        using var db = new LocalDbContext();
        return db.Users.Any(u => u.Email == email && !u.IsDeleted);
    }

    public bool EmailExists(string email, int excludeUserId = 0)
    {
        using var db = new LocalDbContext();
        return db.Users.Any(u => u.Email == email && u.Id != excludeUserId && !u.IsDeleted);
    }

    public User? CreateUser(string username, string email, string password)
    {
        using var db = new LocalDbContext();

        if (db.Users.Any(u => u.Email == email && !u.IsDeleted))
            return null;

        var user = new User
        {
            Guid = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Cashier",
            IsActive = true,
            IsSynced = false,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public bool ValidatePassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public bool ChangePassword(string email, string oldPassword, string newPassword)
    {
        using var db = new LocalDbContext();
        var user = db.Users.FirstOrDefault(u => u.Email == email && !u.IsDeleted);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.IsSynced = false;
        db.SaveChanges();
        return true;
    }
}
