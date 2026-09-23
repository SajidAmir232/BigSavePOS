using POSApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace POSApp.Data.Services
{
    public static class MockDataService
    {
        public static void SeedAll(LocalDbContext db)
        {
            if (db.Products.Any() || db.Customers.Any())
                return;

            SeedProducts(db);
            SeedCustomers(db);
            SeedSuppliers(db);
            SeedChartOfAccounts(db);
            SeedInvoices(db);
            SeedPurchases(db);
            SeedCustomerPayments(db);
            SeedSupplierPayments(db);
            SeedExpenses(db);
            SeedJournalEntries(db);
            SeedInvoiceReturns(db);

            db.SaveChanges();
        }

        private static void SeedProducts(LocalDbContext db)
        {
            var products = new List<Product>
            {
                new() { Guid = Guid.NewGuid(), Name = "Samsung Galaxy S24 Ultra", Sku = "SAM-S24U", Barcode = "8901234567001", Category = "Mobile", PurchasePrice = 85000, SalePrice = 105000, Quantity = 15, IMEI = "353456789012345" },
                new() { Guid = Guid.NewGuid(), Name = "Samsung Galaxy S24+", Sku = "SAM-S24P", Barcode = "8901234567002", Category = "Mobile", PurchasePrice = 65000, SalePrice = 82000, Quantity = 20, IMEI = "353456789012346" },
                new() { Guid = Guid.NewGuid(), Name = "iPhone 15 Pro Max", Sku = "APL-15PM", Barcode = "8901234567003", Category = "Mobile", PurchasePrice = 195000, SalePrice = 235000, Quantity = 10, IMEI = "353456789012347" },
                new() { Guid = Guid.NewGuid(), Name = "iPhone 15", Sku = "APL-15", Barcode = "8901234567004", Category = "Mobile", PurchasePrice = 115000, SalePrice = 140000, Quantity = 12, IMEI = "353456789012348" },
                new() { Guid = Guid.NewGuid(), Name = "Xiaomi Redmi Note 13", Sku = "XMI-RN13", Barcode = "8901234567005", Category = "Mobile", PurchasePrice = 22000, SalePrice = 29000, Quantity = 30, IMEI = "353456789012349" },
                new() { Guid = Guid.NewGuid(), Name = "Samsung Galaxy A54", Sku = "SAM-A54", Barcode = "8901234567006", Category = "Mobile", PurchasePrice = 35000, SalePrice = 45000, Quantity = 18, IMEI = "353456789012350" },
                new() { Guid = Guid.NewGuid(), Name = "OnePlus 12", Sku = "OP-12", Barcode = "8901234567007", Category = "Mobile", PurchasePrice = 75000, SalePrice = 95000, Quantity = 8, IMEI = "353456789012351" },
                new() { Guid = Guid.NewGuid(), Name = "Tecno Spark 20 Pro", Sku = "TEC-SP20P", Barcode = "8901234567008", Category = "Mobile", PurchasePrice = 18000, SalePrice = 24000, Quantity = 25, IMEI = "353456789012352" },
                new() { Guid = Guid.NewGuid(), Name = "AirPods Pro 2", Sku = "APL-APP2", Barcode = "8901234567009", Category = "Accessories", PurchasePrice = 28000, SalePrice = 38000, Quantity = 40 },
                new() { Guid = Guid.NewGuid(), Name = "Samsung 25W Charger", Sku = "SAM-CHG25", Barcode = "8901234567010", Category = "Accessories", PurchasePrice = 2500, SalePrice = 4500, Quantity = 100 },
                new() { Guid = Guid.NewGuid(), Name = "Tempered Glass iPhone 15", Sku = "ACC-TG-15", Barcode = "8901234567011", Category = "Accessories", PurchasePrice = 300, SalePrice = 800, Quantity = 200 },
                new() { Guid = Guid.NewGuid(), Name = "Silicon Case Samsung S24", Sku = "ACC-CS24", Barcode = "8901234567012", Category = "Accessories", PurchasePrice = 500, SalePrice = 1200, Quantity = 150 },
                new() { Guid = Guid.NewGuid(), Name = "USB-C to Lightning Cable", Sku = "ACC-CBL", Barcode = "8901234567013", Category = "Accessories", PurchasePrice = 200, SalePrice = 600, Quantity = 80 },
                new() { Guid = Guid.NewGuid(), Name = "JBL Tune 510BT Headphones", Sku = "JBL-510", Barcode = "8901234567014", Category = "Accessories", PurchasePrice = 4500, SalePrice = 7000, Quantity = 25 },
                new() { Guid = Guid.NewGuid(), Name = "Samsung Galaxy Watch 6", Sku = "SAM-GW6", Barcode = "8901234567015", Category = "Wearable", PurchasePrice = 32000, SalePrice = 42000, Quantity = 12, IMEI = "353456789012353" },
                new() { Guid = Guid.NewGuid(), Name = "Infinix Hot 40 Pro", Sku = "INX-H40P", Barcode = "8901234567016", Category = "Mobile", PurchasePrice = 20000, SalePrice = 27000, Quantity = 22, IMEI = "353456789012354" },
                new() { Guid = Guid.NewGuid(), Name = "Realme 12 Pro", Sku = "RLM-12P", Barcode = "8901234567017", Category = "Mobile", PurchasePrice = 28000, SalePrice = 36000, Quantity = 14, IMEI = "353456789012355" },
                new() { Guid = Guid.NewGuid(), Name = "POCO X6 Pro", Sku = "POC-X6P", Barcode = "8901234567018", Category = "Mobile", PurchasePrice = 30000, SalePrice = 39000, Quantity = 16, IMEI = "353456789012356" },
                new() { Guid = Guid.NewGuid(), Name = "Screen Protector Pack", Sku = "ACC-SPK", Barcode = "8901234567019", Category = "Accessories", PurchasePrice = 100, SalePrice = 350, Quantity = 500 },
                new() { Guid = Guid.NewGuid(), Name = "Phone Stand Holder", Sku = "ACC-PSH", Barcode = "8901234567020", Category = "Accessories", PurchasePrice = 150, SalePrice = 500, Quantity = 200 }
            };

            db.Products.AddRange(products);
            db.SaveChanges();
        }

        private static void SeedCustomers(LocalDbContext db)
        {
            var customers = new List<Customer>
            {
                new() { Guid = Guid.NewGuid(), Name = "Ahmed Khan", Phone = "0301-1234567", Address = "Gulshan-e-Iqbal, Karachi", OpeningBalance = 0, CurrentBalance = 0, CreditLimit = 100000 },
                new() { Guid = Guid.NewGuid(), Name = "Fatima Ali", Phone = "0321-9876543", Address = "DHA Phase 5, Lahore", OpeningBalance = 5000, CurrentBalance = 5000, CreditLimit = 200000 },
                new() { Guid = Guid.NewGuid(), Name = "Usman Sheikh", Phone = "0333-5551234", Address = "F-8, Islamabad", OpeningBalance = 0, CurrentBalance = 0, CreditLimit = 150000 },
                new() { Guid = Guid.NewGuid(), Name = "Sara Malik", Phone = "0345-6789012", Address = "Clifton, Karachi", OpeningBalance = 12000, CurrentBalance = 12000, CreditLimit = 300000 },
                new() { Guid = Guid.NewGuid(), Name = "Hassan Raza", Phone = "0312-3456789", Address = "Johar Town, Lahore", OpeningBalance = 0, CurrentBalance = 0, CreditLimit = 100000 },
                new() { Guid = Guid.NewGuid(), Name = "Ayesha Noor", Phone = "0300-1112233", Address = "Satellite Town, Rawalpindi", OpeningBalance = 8000, CurrentBalance = 8000, CreditLimit = 120000 },
                new() { Guid = Guid.NewGuid(), Name = "Bilal Ahmed", Phone = "0322-4445566", Address = "Saddar, Karachi", OpeningBalance = 0, CurrentBalance = 0, CreditLimit = 80000 },
                new() { Guid = Guid.NewGuid(), Name = "Zainab Bibi", Phone = "0344-7778899", Address = "Gulberg III, Lahore", OpeningBalance = 3000, CurrentBalance = 3000, CreditLimit = 100000 },
                new() { Guid = Guid.NewGuid(), Name = "Omar Farooq", Phone = "0311-2223344", Address = "Blue Area, Islamabad", OpeningBalance = 0, CurrentBalance = 0, CreditLimit = 250000 },
                new() { Guid = Guid.NewGuid(), Name = "Hira Shah", Phone = "0335-9990011", Address = "PECHS, Karachi", OpeningBalance = 15000, CurrentBalance = 15000, CreditLimit = 200000 }
            };

            db.Customers.AddRange(customers);
            db.SaveChanges();
        }

        private static void SeedSuppliers(LocalDbContext db)
        {
            var suppliers = new List<Supplier>
            {
                new() { Guid = Guid.NewGuid(), Name = "Samsung Pakistan (Official)", Phone = "021-111123456", Address = "SITE Area, Karachi", OpeningBalance = 0, PayableBalance = 0 },
                new() { Guid = Guid.NewGuid(), Name = "Apple authorized Distributor - Airlink", Phone = "021-35678901", Address = "Saddar, Karachi", OpeningBalance = 0, PayableBalance = 0 },
                new() { Guid = Guid.NewGuid(), Name = "Xiaomi Pakistan", Phone = "042-35671234", Address = "Mall Road, Lahore", OpeningBalance = 0, PayableBalance = 0 },
                new() { Guid = Guid.NewGuid(), Name = "Techno Mobile Pakistan", Phone = "021-34567890", Address = "Tariq Road, Karachi", OpeningBalance = 0, PayableBalance = 0 },
                new() { Guid = Guid.NewGuid(), Name = "OnePlus Pakistan (Airweb)", Phone = "0300-5678901", Address = "Gulberg, Lahore", OpeningBalance = 0, PayableBalance = 0 },
                new() { Guid = Guid.NewGuid(), Name = "Infinix Pakistan", Phone = "042-37890123", Address = "DHA, Lahore", OpeningBalance = 0, PayableBalance = 0 },
                new() { Guid = Guid.NewGuid(), Name = "Accessories Hub (Wholesale)", Phone = "0321-6789012", Address = "Board Market, Karachi", OpeningBalance = 0, PayableBalance = 0 },
                new() { Guid = Guid.NewGuid(), Name = "Mobile Parts & Tools Co.", Phone = "0333-8901234", Address = "Shah Alam Market, Lahore", OpeningBalance = 0, PayableBalance = 0 }
            };

            db.Suppliers.AddRange(suppliers);
            db.SaveChanges();
        }

        private static void SeedChartOfAccounts(LocalDbContext db)
        {
            var accounts = new List<ChartOfAccount>
            {
                new() { Guid = Guid.NewGuid(), AccountName = "Cash in Hand", AccountType = "Cash" },
                new() { Guid = Guid.NewGuid(), AccountName = "Bank Alfalah Current", AccountType = "Bank" },
                new() { Guid = Guid.NewGuid(), AccountName = "Sales Revenue", AccountType = "Sales" },
                new() { Guid = Guid.NewGuid(), AccountName = "Cost of Goods Sold", AccountType = "Expenses" },
                new() { Guid = Guid.NewGuid(), AccountName = "Accounts Receivable", AccountType = "Receivables" },
                new() { Guid = Guid.NewGuid(), AccountName = "Accounts Payable", AccountType = "Payables" },
                new() { Guid = Guid.NewGuid(), AccountName = "Purchase Account", AccountType = "Purchases" },
                new() { Guid = Guid.NewGuid(), AccountName = "Office Expenses", AccountType = "Expenses" },
                new() { Guid = Guid.NewGuid(), AccountName = "Rent Expense", AccountType = "Expenses" },
                new() { Guid = Guid.NewGuid(), AccountName = "Salary Expense", AccountType = "Expenses" },
                new() { Guid = Guid.NewGuid(), AccountName = "Utility Bills", AccountType = "Expenses" },
                new() { Guid = Guid.NewGuid(), AccountName = "Inventory Asset", AccountType = "Cash" }
            };

            db.ChartOfAccounts.AddRange(accounts);
            db.SaveChanges();
        }

        private static void SeedInvoices(LocalDbContext db)
        {
            var customers = db.Customers.ToList();
            var products = db.Products.Where(p => p.Category != "Service").ToList();
            if (customers.Count == 0 || products.Count == 0) return;

            var random = new Random(42);

            for (int i = 0; i < 25; i++)
            {
                var daysAgo = random.Next(0, 45);
                var invoiceDate = DateTime.Now.AddDays(-daysAgo);
                var customer = customers[random.Next(customers.Count)];

                var invoice = new Invoice
                {
                    Guid = Guid.NewGuid(),
                    InvoiceNumber = $"INV-{DateTime.Now.AddDays(-daysAgo):yyyyMMdd}-{i + 1:D4}",
                    CustomerGuid = customer.Guid,
                    InvoiceDate = invoiceDate,
                    CreatedByDeviceId = Environment.MachineName,
                    Items = new List<InvoiceItem>(),
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                };

                decimal subtotal = 0;
                var itemCount = random.Next(1, 4);
                for (int j = 0; j < itemCount; j++)
                {
                    var product = products[random.Next(products.Count)];
                    var quantity = random.Next(1, 4);
                    var itemTotal = product.SalePrice * quantity;

                    invoice.Items.Add(new InvoiceItem
                    {
                        Guid = Guid.NewGuid(),
                        ProductGuid = product.Guid,
                        ProductName = product.Name,
                        IMEI = product.IMEI,
                        Quantity = quantity,
                        UnitPrice = product.SalePrice,
                        Total = itemTotal
                    });

                    subtotal += itemTotal;
                }

                var discount = random.Next(0, 3) == 0 ? Math.Round(subtotal * 0.05m, 0) : 0;
                var tax = Math.Round((subtotal - discount) * 0.17m, 0);
                var grandTotal = subtotal - discount + tax;
                var isPartial = random.Next(0, 4) == 0;
                var paidAmount = isPartial ? grandTotal * random.Next(3, 8) / 10m : grandTotal;

                invoice.SubTotal = subtotal;
                invoice.Discount = discount;
                invoice.Tax = tax;
                invoice.GrandTotal = grandTotal;
                invoice.PaidAmount = Math.Round(paidAmount, 0);
                invoice.SaleType = isPartial ? "Credit" : "Cash";
                invoice.Status = isPartial ? (invoice.PaidAmount >= grandTotal ? "Paid" : "Partial") : "Paid";

                db.Invoices.Add(invoice);
            }

            db.SaveChanges();
        }

        private static void SeedPurchases(LocalDbContext db)
        {
            var suppliers = db.Suppliers.ToList();
            var products = db.Products.Where(p => p.Category != "Service").ToList();
            if (suppliers.Count == 0 || products.Count == 0) return;

            var random = new Random(42);

            for (int i = 0; i < 15; i++)
            {
                var daysAgo = random.Next(0, 45);
                var purchaseDate = DateTime.Now.AddDays(-daysAgo);
                var supplier = suppliers[random.Next(suppliers.Count)];

                var purchase = new PurchaseInvoice
                {
                    Guid = Guid.NewGuid(),
                    PurchaseNumber = $"PUR-{DateTime.Now.AddDays(-daysAgo):yyyyMMdd}-{i + 1:D4}",
                    SupplierGuid = supplier.Guid,
                    PurchaseDate = purchaseDate,
                    Items = new List<PurchaseInvoiceItem>(),
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                };

                decimal subtotal = 0;
                var itemCount = random.Next(1, 4);
                for (int j = 0; j < itemCount; j++)
                {
                    var product = products[random.Next(products.Count)];
                    var quantity = random.Next(5, 25);
                    var itemTotal = product.PurchasePrice * quantity;

                    purchase.Items.Add(new PurchaseInvoiceItem
                    {
                        Guid = Guid.NewGuid(),
                        ProductGuid = product.Guid,
                        ProductName = product.Name,
                        Quantity = quantity,
                        CostPrice = product.PurchasePrice,
                        Total = itemTotal
                    });

                    subtotal += itemTotal;
                }

                purchase.SubTotal = subtotal;
                var paidRatio = random.Next(0, 3) == 0 ? 0.5m : 1.0m;
                purchase.PaidAmount = Math.Round(subtotal * paidRatio, 0);
                purchase.Status = purchase.PaidAmount >= subtotal ? "Paid" : "Unpaid";

                db.PurchaseInvoices.Add(purchase);
            }

            db.SaveChanges();
        }

        private static void SeedCustomerPayments(LocalDbContext db)
        {
            var customers = db.Customers.ToList();
            var invoices = db.Invoices.Where(i => i.Status == "Partial" || i.Status == "Credit").ToList();
            if (customers.Count == 0) return;

            var random = new Random(42);

            foreach (var invoice in invoices.Take(8))
            {
                var remaining = invoice.GrandTotal - invoice.PaidAmount;
                if (remaining <= 0) continue;

                var paymentAmount = Math.Round(remaining * random.Next(3, 10) / 10m, 0);
                if (paymentAmount <= 0) continue;

                db.CustomerPayments.Add(new CustomerPayment
                {
                    Guid = Guid.NewGuid(),
                    CustomerGuid = invoice.CustomerGuid,
                    Amount = paymentAmount,
                    PaymentDate = invoice.InvoiceDate.AddDays(random.Next(1, 10)),
                    Note = $"Payment against {invoice.InvoiceNumber}",
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                });
            }

            db.SaveChanges();
        }

        private static void SeedSupplierPayments(LocalDbContext db)
        {
            var purchases = db.PurchaseInvoices.Where(p => p.Status == "Unpaid" || p.PaidAmount < p.SubTotal).ToList();
            if (purchases.Count == 0) return;

            var random = new Random(42);

            foreach (var purchase in purchases.Take(5))
            {
                var remaining = purchase.SubTotal - purchase.PaidAmount;
                if (remaining <= 0) continue;

                var paymentAmount = Math.Round(remaining * random.Next(3, 8) / 10m, 0);
                if (paymentAmount <= 0) continue;

                db.SupplierPayments.Add(new SupplierPayment
                {
                    Guid = Guid.NewGuid(),
                    SupplierGuid = purchase.SupplierGuid,
                    Amount = paymentAmount,
                    PaymentDate = purchase.PurchaseDate.AddDays(random.Next(5, 20)),
                    Note = $"Payment against {purchase.PurchaseNumber}",
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                });
            }

            db.SaveChanges();
        }

        private static void SeedExpenses(LocalDbContext db)
        {
            var categories = new[] { "Rent", "Utilities", "Salaries", "Transport", "Maintenance", "Office Supplies", "Marketing", "Internet" };
            var notes = new[]
            {
                "Monthly shop rent",
                "Electricity bill - June",
                "Staff salaries - June",
                "Fuel for delivery bike",
                "AC repair and maintenance",
                "Printer cartridges and paper",
                "Facebook ads campaign",
                "PTCL internet bill"
            };

            var random = new Random(42);

            for (int i = 0; i < 20; i++)
            {
                var daysAgo = random.Next(0, 45);
                var catIndex = random.Next(categories.Length);

                var amounts = new Dictionary<string, decimal>
                {
                    ["Rent"] = 50000,
                    ["Utilities"] = random.Next(5000, 15000),
                    ["Salaries"] = 25000,
                    ["Transport"] = random.Next(1000, 5000),
                    ["Maintenance"] = random.Next(2000, 8000),
                    ["Office Supplies"] = random.Next(500, 3000),
                    ["Marketing"] = random.Next(3000, 10000),
                    ["Internet"] = 3500
                };

                db.Expenses.Add(new Expense
                {
                    Guid = Guid.NewGuid(),
                    Amount = amounts[categories[catIndex]],
                    Category = categories[catIndex],
                    Note = notes[catIndex],
                    ExpenseDate = DateTime.Now.AddDays(-daysAgo),
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                });
            }

            db.SaveChanges();
        }

        private static void SeedJournalEntries(LocalDbContext db)
        {
            var accounts = db.ChartOfAccounts.ToList();
            if (accounts.Count < 4) return;

            var cashAccount = accounts.FirstOrDefault(a => a.AccountName == "Cash in Hand");
            var salesAccount = accounts.FirstOrDefault(a => a.AccountName == "Sales Revenue");
            var purchaseAccount = accounts.FirstOrDefault(a => a.AccountName == "Purchase Account");
            var bankAccount = accounts.FirstOrDefault(a => a.AccountName.Contains("Bank"));

            if (cashAccount == null || salesAccount == null || purchaseAccount == null) return;

            var invoices = db.Invoices.Take(10).ToList();
            foreach (var inv in invoices)
            {
                db.JournalEntries.Add(new JournalEntry
                {
                    Guid = Guid.NewGuid(),
                    DebitAccountGuid = cashAccount.Guid,
                    CreditAccountGuid = salesAccount.Guid,
                    Amount = inv.GrandTotal,
                    Description = $"Sale {inv.InvoiceNumber}",
                    EntryDate = inv.InvoiceDate,
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                });
            }

            var purchases = db.PurchaseInvoices.Take(8).ToList();
            foreach (var pur in purchases)
            {
                db.JournalEntries.Add(new JournalEntry
                {
                    Guid = Guid.NewGuid(),
                    DebitAccountGuid = purchaseAccount.Guid,
                    CreditAccountGuid = bankAccount?.Guid ?? cashAccount.Guid,
                    Amount = pur.SubTotal,
                    Description = $"Purchase {pur.PurchaseNumber}",
                    EntryDate = pur.PurchaseDate,
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                });
            }

            db.SaveChanges();
        }

        private static void SeedInvoiceReturns(LocalDbContext db)
        {
            var invoices = db.Invoices.Include(i => i.Items).Where(i => !i.IsDeleted).ToList();
            if (invoices.Count == 0) return;

            var random = new Random(42);

            foreach (var invoice in invoices.Take(4))
            {
                if (invoice.Items.Count == 0) continue;

                var item = invoice.Items[random.Next(invoice.Items.Count)];
                var returnQty = random.Next(1, (int)item.Quantity + 1);

                db.InvoiceReturns.Add(new InvoiceReturn
                {
                    Guid = Guid.NewGuid(),
                    OriginalInvoiceGuid = invoice.Guid,
                    ProductGuid = item.ProductGuid,
                    IMEI = item.IMEI,
                    Quantity = returnQty,
                    Reason = random.Next(2) == 0 ? "Defective product" : "Customer changed mind",
                    ReturnDate = invoice.InvoiceDate.AddDays(random.Next(1, 5)),
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSynced = false
                });
            }

            db.SaveChanges();
        }
    }
}
