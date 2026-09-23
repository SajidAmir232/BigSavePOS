using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data
{
    public class LocalDbContext : DbContext
    {
        private readonly string _dbPath;

        public LocalDbContext()
        {
            var configured = Environment.GetEnvironmentVariable("POSAPP_DB_PATH");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                var full = Path.GetFullPath(configured);
                Directory.CreateDirectory(Path.GetDirectoryName(full)!);
                _dbPath = full;
            }
            else
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "POSApp");
                Directory.CreateDirectory(folder);
                _dbPath = Path.Combine(folder, "pos_local.db");
            }
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<InvoiceReturn> InvoiceReturns => Set<InvoiceReturn>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
        public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
        public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();
        public DbSet<User> Users => Set<User>();
        public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
        public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ImeiUnit> ImeiUnits => Set<ImeiUnit>();
        public DbSet<RepairJob> RepairJobs => Set<RepairJob>();
        public DbSet<RepairJobPart> RepairJobParts => Set<RepairJobPart>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<UserBranch> UserBranches => Set<UserBranch>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<HeldSale> HeldSales => Set<HeldSale>();
        public DbSet<HeldSaleItem> HeldSaleItems => Set<HeldSaleItem>();
        public DbSet<SalePayment> SalePayments => Set<SalePayment>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath};Pooling=false");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique indexes on all Guid columns
            modelBuilder.Entity<Branch>().HasIndex(b => b.Guid).IsUnique();
            modelBuilder.Entity<Branch>().HasIndex(b => b.Code).IsUnique();
            modelBuilder.Entity<InventoryTransaction>().HasIndex(t => t.Guid).IsUnique();
            modelBuilder.Entity<HeldSale>().HasIndex(h => h.Guid).IsUnique();
            modelBuilder.Entity<SalePayment>().HasIndex(p => p.Guid).IsUnique();
            modelBuilder.Entity<AuditLog>().HasIndex(a => a.Guid).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Guid).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => c.Guid).IsUnique();
            modelBuilder.Entity<Invoice>().HasIndex(i => i.Guid).IsUnique();
            modelBuilder.Entity<InvoiceItem>().HasIndex(ii => ii.Guid).IsUnique();
            modelBuilder.Entity<InvoiceReturn>().HasIndex(ir => ir.Guid).IsUnique();
            modelBuilder.Entity<Supplier>().HasIndex(s => s.Guid).IsUnique();
            modelBuilder.Entity<PurchaseInvoice>().HasIndex(pi => pi.Guid).IsUnique();
            modelBuilder.Entity<PurchaseInvoiceItem>().HasIndex(pii => pii.Guid).IsUnique();
            modelBuilder.Entity<SupplierPayment>().HasIndex(sp => sp.Guid).IsUnique();
            modelBuilder.Entity<CustomerPayment>().HasIndex(cp => cp.Guid).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Guid).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<CompanySettings>().HasKey(cs => cs.Id);
            modelBuilder.Entity<ChartOfAccount>().HasIndex(coa => coa.Guid).IsUnique();
            modelBuilder.Entity<JournalEntry>().HasIndex(je => je.Guid).IsUnique();
            modelBuilder.Entity<Expense>().HasIndex(e => e.Guid).IsUnique();
            modelBuilder.Entity<ImeiUnit>().HasIndex(i => i.Guid).IsUnique();
            modelBuilder.Entity<ImeiUnit>().HasIndex(i => i.SerialNumber).IsUnique();
            modelBuilder.Entity<RepairJob>().HasIndex(r => r.Guid).IsUnique();
            modelBuilder.Entity<RepairJobPart>().HasIndex(rp => rp.Guid).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => new { p.BranchId, p.Barcode }).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => new { p.BranchId, p.Sku }).IsUnique();
            modelBuilder.Entity<UserBranch>().HasIndex(ub => new { ub.UserId, ub.BranchId }).IsUnique();

            // Invoice -> InvoiceItems relationship
            modelBuilder.Entity<InvoiceItem>()
                .Property<int>("InvoiceId")
                .IsRequired();
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.Items)
                .WithOne()
                .HasForeignKey("InvoiceId")
                .OnDelete(DeleteBehavior.Cascade);

            // PurchaseInvoice -> PurchaseInvoiceItems relationship
            modelBuilder.Entity<PurchaseInvoiceItem>()
                .Property<int>("PurchaseInvoiceId")
                .IsRequired();
            modelBuilder.Entity<PurchaseInvoice>()
                .HasMany(pi => pi.Items)
                .WithOne()
                .HasForeignKey("PurchaseInvoiceId")
                .OnDelete(DeleteBehavior.Cascade);

            // RepairJob -> RepairJobParts relationship
            modelBuilder.Entity<RepairJobPart>()
                .Property<int>("RepairJobId")
                .IsRequired();
            modelBuilder.Entity<RepairJob>()
                .HasMany(r => r.Parts)
                .WithOne()
                .HasForeignKey("RepairJobId")
                .OnDelete(DeleteBehavior.Cascade);

            // Held-sale relationships
            modelBuilder.Entity<HeldSale>().HasMany(h => h.Items).WithOne().HasForeignKey("HeldSaleId").OnDelete(DeleteBehavior.Cascade);

            // Seed default branch and admin user
            modelBuilder.Entity<Branch>().HasData(new Branch
            { Id = 1, Guid = Guid.Parse("00000000-0000-0000-0000-000000000010"), Name = "Main Branch", Code = "MAIN", IsActive = true, UpdatedAtUtc = DateTime.UtcNow });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Guid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin",
                Email = "admin@posapp.local",
                PasswordHash = "$2a$11$BoDXkSEX7DtRJ7klkgHhQ.RpZLtIXZ5dw.Vz/MMreEnMIoYCLa2fu",
                Role = "Admin",
                IsActive = true,
                IsDeleted = false,
                IsSynced = true,
                FailedAttemptCount = 0,
                ForcePasswordChange = false,
                PrimaryBranchId = 1,
                SubscriptionStartDate = DateTime.UtcNow,
                SubscriptionEndDate = DateTime.UtcNow.AddYears(10),
                AssignedBy = "System",
                UpdatedAtUtc = DateTime.UtcNow
            });

            // Seed default company settings
            modelBuilder.Entity<CompanySettings>().HasData(new CompanySettings
            {
                Id = 1,
                CompanyName = "DEMO - Tech Mobile Shop",
                Address = "123 Business Street, Karachi, Pakistan",
                TaxNumber = "NTN-123456789",
                TaxRatePercent = 17m,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }
    }
}
