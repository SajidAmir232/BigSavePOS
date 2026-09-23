using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using POSApp.Data;

namespace POSApp.Data.Services;

/// Adds v1 POS columns/tables to an existing local SQLite database without destroying user data.
public static class DatabaseSchemaBootstrapper
{
    public static void Ensure(LocalDbContext db)
    {
        db.Database.EnsureCreated();
        using var connection = new SqliteConnection(db.Database.GetConnectionString());
        connection.Open();
        void Exec(string sql) { using var c = connection.CreateCommand(); c.CommandText = sql; c.ExecuteNonQuery(); }
        bool HasColumn(string table, string column) { using var c = connection.CreateCommand(); c.CommandText = $"PRAGMA table_info([{table}])"; using var r = c.ExecuteReader(); while (r.Read()) if (string.Equals(r.GetString(1), column, StringComparison.OrdinalIgnoreCase)) return true; return false; }
        void Add(string table, string column, string type, string defaultSql, bool nullable = false) { if (!HasColumn(table, column)) Exec($"ALTER TABLE [{table}] ADD COLUMN [{column}] {type} {(nullable ? "NULL" : "NOT NULL")} DEFAULT {defaultSql}"); }

        Add("Products", "Subcategory", "TEXT", "''"); Add("Products", "Brand", "TEXT", "''"); Add("Products", "WholesalePrice", "TEXT", "0"); Add("Products", "MinimumStock", "INTEGER", "10"); Add("Products", "ExpiryDate", "TEXT", "NULL", true); Add("Products", "ImageUrl", "TEXT", "NULL", true); Add("Products", "BranchId", "INTEGER", "1");
        Add("Users", "PrimaryBranchId", "INTEGER", "1");
        Add("Invoices", "BranchId", "INTEGER", "1"); Add("PurchaseInvoices", "BranchId", "INTEGER", "1"); Add("Invoices", "TaxRatePercent", "TEXT", "0"); Add("Invoices", "PaymentStatus", "TEXT", "'Paid'");
        Add("CompanySettings", "ShowCustomerField", "INTEGER", "1");
        Add("CompanySettings", "RequireExpiryDate", "INTEGER", "0");
        Exec("UPDATE Products SET Barcode = TRIM(Barcode, char(39) || char(34) || ' ') WHERE Barcode IS NOT NULL");

        Exec("CREATE TABLE IF NOT EXISTS Branches (Id INTEGER PRIMARY KEY AUTOINCREMENT, Guid TEXT NOT NULL UNIQUE, Name TEXT NOT NULL, Code TEXT NOT NULL UNIQUE, Address TEXT NULL, Phone TEXT NULL, IsActive INTEGER NOT NULL DEFAULT 1, UpdatedAtUtc TEXT NOT NULL)");
        Exec("INSERT OR IGNORE INTO Branches(Id, Guid, Name, Code, IsActive, UpdatedAtUtc) VALUES(1, '00000000-0000-0000-0000-000000000010', 'Main Branch', 'MAIN', 1, datetime('now'))");

        // Repair the bundled demo admin credential when upgrading an older database.
        // This only touches the known seeded admin record and does not overwrite a
        // custom admin password.
        var admin = db.Users.FirstOrDefault(u => u.Id == 1 || u.Username.ToLower() == "admin");
        if (admin == null)
        {
            db.Users.Add(new Models.User
            {
                Id = 1, Guid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin", Email = "admin@posapp.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin",
                IsActive = true, IsDeleted = false, IsSynced = false, FailedAttemptCount = 0,
                ForcePasswordChange = false, PrimaryBranchId = 1,
                SubscriptionStartDate = DateTime.UtcNow, SubscriptionEndDate = DateTime.UtcNow.AddYears(10),
                AssignedBy = "System", UpdatedAtUtc = DateTime.UtcNow
            });
            db.SaveChanges();
        }
        else if (admin.Id == 1 && admin.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            const string knownOldSeedHash = "$2a$11$BoDXkSEX7DtRJ7klkgHhQ.RpZLtIXZ5dw.Vz/MMreEnMIoYCLa2fu";
            if (string.IsNullOrWhiteSpace(admin.PasswordHash) || admin.PasswordHash == knownOldSeedHash)
            {
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                admin.IsActive = true;
                admin.IsDeleted = false;
                admin.FailedAttemptCount = 0;
                admin.LastFailedAttempt = null;
                admin.PrimaryBranchId ??= 1;
                admin.SubscriptionEndDate ??= DateTime.UtcNow.AddYears(10);
                admin.UpdatedAtUtc = DateTime.UtcNow;
                db.SaveChanges();
            }
        }
        Exec("CREATE TABLE IF NOT EXISTS UserBranches (Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId INTEGER NOT NULL, BranchId INTEGER NOT NULL, IsPrimary INTEGER NOT NULL DEFAULT 0, AssignedAtUtc TEXT NOT NULL, UNIQUE(UserId, BranchId))");
        Exec("INSERT OR IGNORE INTO UserBranches(UserId, BranchId, IsPrimary, AssignedAtUtc) SELECT Id, COALESCE(PrimaryBranchId,1), 1, datetime('now') FROM Users");
        Exec("CREATE TABLE IF NOT EXISTS InventoryTransactions (Id INTEGER PRIMARY KEY AUTOINCREMENT, Guid TEXT NOT NULL UNIQUE, ProductGuid TEXT NOT NULL, BranchId INTEGER NOT NULL, QuantityChange TEXT NOT NULL, QuantityAfter INTEGER NOT NULL, Type TEXT NOT NULL, Reference TEXT NULL, Reason TEXT NULL, CreatedBy TEXT NULL, CreatedAtUtc TEXT NOT NULL)");
        Exec("CREATE TABLE IF NOT EXISTS HeldSales (Id INTEGER PRIMARY KEY AUTOINCREMENT, Guid TEXT NOT NULL UNIQUE, BranchId INTEGER NOT NULL, CustomerGuid TEXT NULL, Label TEXT NOT NULL, Discount TEXT NOT NULL DEFAULT 0, Tax TEXT NOT NULL DEFAULT 0, HeldAtUtc TEXT NOT NULL, HeldBy TEXT NULL)");
        Exec("CREATE TABLE IF NOT EXISTS HeldSaleItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, HeldSaleId INTEGER NOT NULL, HeldSaleGuid TEXT NOT NULL, ProductGuid TEXT NOT NULL, ProductName TEXT NOT NULL, Quantity TEXT NOT NULL, UnitPrice TEXT NOT NULL, FOREIGN KEY(HeldSaleId) REFERENCES HeldSales(Id) ON DELETE CASCADE)");
        Exec("CREATE TABLE IF NOT EXISTS SalePayments (Id INTEGER PRIMARY KEY AUTOINCREMENT, Guid TEXT NOT NULL UNIQUE, InvoiceGuid TEXT NOT NULL, Method TEXT NOT NULL, Amount TEXT NOT NULL, Reference TEXT NULL, PaidAtUtc TEXT NOT NULL)");
        Exec("CREATE TABLE IF NOT EXISTS AuditLogs (Id INTEGER PRIMARY KEY AUTOINCREMENT, Guid TEXT NOT NULL UNIQUE, UserId INTEGER NULL, BranchId INTEGER NULL, Action TEXT NOT NULL, Entity TEXT NOT NULL, EntityId TEXT NULL, Details TEXT NULL, CreatedAtUtc TEXT NOT NULL)");
        Exec("CREATE INDEX IF NOT EXISTS IX_Products_Branch_Barcode ON Products(BranchId, Barcode)");
        Exec("CREATE INDEX IF NOT EXISTS IX_Products_Branch_Sku ON Products(BranchId, Sku)");
        Exec("CREATE INDEX IF NOT EXISTS IX_InventoryTransactions_ProductBranch ON InventoryTransactions(ProductGuid, BranchId)");
        Exec("CREATE INDEX IF NOT EXISTS IX_AuditLogs_CreatedAtUtc ON AuditLogs(CreatedAtUtc)");
    }
}
