using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

/// <summary>
/// Advanced reporting service with AR aging, profitability, and inventory analytics
/// </summary>
public class AdvancedReportingService
{
    // ==================== AR Aging Analysis ====================

    public class ArAgingBucket
    {
        public string BucketName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public List<ArAgingBucket> GetCustomerArAging()
    {
        using var db = new LocalDbContext();

        var invoices = db.Invoices
            .Where(i => i.Status != "Paid" && !i.IsDeleted && i.SaleType == "Credit")
            .ToList();

        var today = DateTime.Now;
        var buckets = new List<ArAgingBucket>
        {
            new() { BucketName = "Current (0-30 days)" },
            new() { BucketName = "31-60 Days" },
            new() { BucketName = "61-90 Days" },
            new() { BucketName = "90+ Days" }
        };

        foreach (var invoice in invoices)
        {
            var daysOverdue = (int)(today - invoice.InvoiceDate).TotalDays;
            var outstanding = invoice.GrandTotal - invoice.PaidAmount;

            var bucket = daysOverdue switch
            {
                <= 30 => buckets[0],
                <= 60 => buckets[1],
                <= 90 => buckets[2],
                _ => buckets[3]
            };

            bucket.Amount += outstanding;
            bucket.Count++;
        }

        var total = buckets.Sum(b => b.Amount);
        if (total > 0)
        {
            foreach (var bucket in buckets)
                bucket.Percentage = (bucket.Amount / total) * 100;
        }

        return buckets;
    }

    // ==================== AP Aging Analysis ====================

    public List<ArAgingBucket> GetSupplierApAging()
    {
        using var db = new LocalDbContext();

        var invoices = db.PurchaseInvoices
            .Where(pi => pi.Status != "Paid" && !pi.IsDeleted)
            .ToList();

        var today = DateTime.Now;
        var buckets = new List<ArAgingBucket>
        {
            new() { BucketName = "Current (0-30 days)" },
            new() { BucketName = "31-60 Days" },
            new() { BucketName = "61-90 Days" },
            new() { BucketName = "90+ Days" }
        };

        foreach (var invoice in invoices)
        {
            var daysOverdue = (int)(today - invoice.PurchaseDate).TotalDays;
            var outstanding = invoice.SubTotal - invoice.PaidAmount;

            var bucket = daysOverdue switch
            {
                <= 30 => buckets[0],
                <= 60 => buckets[1],
                <= 90 => buckets[2],
                _ => buckets[3]
            };

            bucket.Amount += outstanding;
            bucket.Count++;
        }

        var total = buckets.Sum(b => b.Amount);
        if (total > 0)
        {
            foreach (var bucket in buckets)
                bucket.Percentage = (bucket.Amount / total) * 100;
        }

        return buckets;
    }

    // ==================== Profitability Analysis ====================

    public class ProfitabilityMetrics
    {
        public decimal TotalSales { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetProfitMargin { get; set; }
        public int TransactionCount { get; set; }
    }

    public ProfitabilityMetrics GetProfitabilityMetrics(DateTime fromDate, DateTime toDate)
    {
        using var db = new LocalDbContext();

        var invoices = db.Invoices
            .Where(i => i.InvoiceDate.Date >= fromDate.Date && 
                        i.InvoiceDate.Date <= toDate.Date && 
                        !i.IsDeleted)
            .Include(i => i.Items)
            .ToList();

        var totalSales = invoices.Sum(i => i.GrandTotal);

        // Calculate COGS from purchase price of sold items
        var totalCogs = 0m;
        foreach (var invoice in invoices)
        {
            foreach (var item in invoice.Items)
            {
                var product = db.Products.FirstOrDefault(p => p.Guid == item.ProductGuid);
                if (product != null)
                {
                    totalCogs += product.PurchasePrice * item.Quantity;
                }
            }
        }

        var grossProfit = totalSales - totalCogs;
        var grossProfitMargin = totalSales > 0 ? (grossProfit / totalSales) * 100 : 0;

        var expenses = db.Expenses
            .Where(e => e.ExpenseDate.Date >= fromDate.Date && 
                       e.ExpenseDate.Date <= toDate.Date && 
                       !e.IsDeleted)
            .ToList();

        var totalExpenses = expenses.Sum(e => e.Amount);
        var netProfit = grossProfit - totalExpenses;
        var netProfitMargin = totalSales > 0 ? (netProfit / totalSales) * 100 : 0;

        return new ProfitabilityMetrics
        {
            TotalSales = totalSales,
            TotalCogs = totalCogs,
            GrossProfit = grossProfit,
            GrossProfitMargin = grossProfitMargin,
            TotalExpenses = totalExpenses,
            NetProfit = netProfit,
            NetProfitMargin = netProfitMargin,
            TransactionCount = invoices.Count
        };
    }

    // ==================== Inventory Analytics ====================

    public class InventoryAnalytics
    {
        public int LowStockCount { get; set; }
        public int OverstockCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal AverageInventoryTurnover { get; set; }
        public List<SlowMovingItem> SlowMovingItems { get; set; } = new();
        public List<FastMovingItem> FastMovingItems { get; set; } = new();
    }

    public class SlowMovingItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public decimal Value { get; set; }
        public int DaysSinceLastSale { get; set; }
    }

    public class FastMovingItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int QuantitySoldLastMonth { get; set; }
        public decimal RevenueLast30Days { get; set; }
        public decimal Turnover { get; set; }
    }

    public InventoryAnalytics GetInventoryAnalytics()
    {
        using var db = new LocalDbContext();

        var products = db.Products.Where(p => !p.IsDeleted).ToList();
        var lastMonth = DateTime.Now.AddMonths(-1);

        var lowStockCount = products.Count(p => p.Quantity < 10);
        var overstockCount = products.Count(p => p.Quantity > 100);
        var totalInventoryValue = products.Sum(p => p.Quantity * p.SalePrice);

        var invoiceItems = db.Invoices
            .Where(i => i.InvoiceDate >= lastMonth && !i.IsDeleted)
            .Include(i => i.Items)
            .SelectMany(i => i.Items)
            .ToList();

        var slowMoving = new List<SlowMovingItem>();
        var fastMoving = new List<FastMovingItem>();

        foreach (var product in products)
        {
            var lastSaleDate = db.Invoices
                .Where(i => !i.IsDeleted)
                .Include(i => i.Items)
                .SelectMany(i => i.Items)
                .Where(item => item.ProductGuid == product.Guid)
                .OrderByDescending(item => item.InvoiceGuid)
                .FirstOrDefault();

            if (lastSaleDate == null)
            {
                slowMoving.Add(new SlowMovingItem
                {
                    ProductName = product.Name,
                    Sku = product.Sku,
                    QuantityInStock = product.Quantity,
                    Value = product.Quantity * product.SalePrice,
                    DaysSinceLastSale = 999
                });
            }
        }

        var salesLastMonth = invoiceItems
            .GroupBy(i => i.ProductGuid)
            .Select(g => new { ProductGuid = g.Key, Quantity = (int)g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.Total) })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToList();

        foreach (var sale in salesLastMonth)
        {
            var product = products.FirstOrDefault(p => p.Guid == sale.ProductGuid);
            if (product != null)
            {
                fastMoving.Add(new FastMovingItem
                {
                    ProductName = product.Name,
                    Sku = product.Sku,
                    QuantitySoldLastMonth = sale.Quantity,
                    RevenueLast30Days = sale.Revenue,
                    Turnover = product.Quantity > 0 ? (decimal)sale.Quantity / product.Quantity : 0
                });
            }
        }

        return new InventoryAnalytics
        {
            LowStockCount = lowStockCount,
            OverstockCount = overstockCount,
            TotalInventoryValue = totalInventoryValue,
            AverageInventoryTurnover = products.Count > 0 ? invoiceItems.Count / (decimal)products.Count : 0,
            SlowMovingItems = slowMoving.OrderByDescending(x => x.DaysSinceLastSale).Take(5).ToList(),
            FastMovingItems = fastMoving.OrderByDescending(x => x.RevenueLast30Days).Take(5).ToList()
        };
    }

    // ==================== Sales Performance ====================

    public class SalesPerformance
    {
        public string Period { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageTransaction { get; set; }
        public string TopCategory { get; set; } = string.Empty;
        public decimal CategoryRevenue { get; set; }
    }

    public List<SalesPerformance> GetSalesPerformanceByDay(int days = 30)
    {
        using var db = new LocalDbContext();

        var result = new List<SalesPerformance>();
        var endDate = DateTime.Now;
        var startDate = endDate.AddDays(-days);

        for (int i = 0; i < days; i++)
        {
            var currentDate = startDate.AddDays(i).Date;
            var nextDate = currentDate.AddDays(1);

            var dayInvoices = db.Invoices
                .Where(inv => inv.InvoiceDate.Date == currentDate && !inv.IsDeleted)
                .Include(i => i.Items)
                .ToList();

            var sales = dayInvoices.Sum(i => i.GrandTotal);
            var count = dayInvoices.Count;
            var avg = count > 0 ? sales / count : 0;

            var topCategory = "N/A";
            var categoryRevenue = 0m;

            var categoryGroups = dayInvoices
                .SelectMany(i => i.Items)
                .GroupBy(item => db.Products.FirstOrDefault(p => p.Guid == item.ProductGuid)?.Category ?? "Unknown")
                .OrderByDescending(g => g.Sum(item => item.Total))
                .FirstOrDefault();

            if (categoryGroups != null)
            {
                topCategory = categoryGroups.Key;
                categoryRevenue = categoryGroups.Sum(item => item.Total);
            }

            result.Add(new SalesPerformance
            {
                Period = currentDate.ToString("yyyy-MM-dd"),
                Sales = sales,
                TransactionCount = count,
                AverageTransaction = avg,
                TopCategory = topCategory,
                CategoryRevenue = categoryRevenue
            });
        }

        return result;
    }
}
