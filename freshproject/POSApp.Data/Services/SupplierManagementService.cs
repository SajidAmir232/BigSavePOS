using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

/// <summary>
/// Enhanced supplier management with performance analytics and relationship tracking
/// </summary>
public class SupplierManagementService
{
    // ==================== Supplier Performance ====================

    public class SupplierPerformance
    {
        public Guid SupplierGuid { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal OnTimePercentage { get; set; }
        public decimal QualityScore { get; set; }
        public decimal OverallRating { get; set; }
        public DateTime LastPurchaseDate { get; set; }
    }

    public List<SupplierPerformance> GetSupplierPerformanceReport()
    {
        using var db = new LocalDbContext();

        var suppliers = db.Suppliers.Where(s => !s.IsDeleted).ToList();
        var performanceList = new List<SupplierPerformance>();

        foreach (var supplier in suppliers)
        {
            var purchases = db.PurchaseInvoices
                .Where(pi => pi.SupplierGuid == supplier.Guid && !pi.IsDeleted)
                .ToList();

            var totalPurchases = purchases.Sum(p => p.SubTotal);
            var totalOrders = purchases.Count;
            var averageOrderValue = totalOrders > 0 ? totalPurchases / totalOrders : 0;

            var lastPurchaseDate = purchases.OrderByDescending(p => p.PurchaseDate).FirstOrDefault()?.PurchaseDate ?? DateTime.MinValue;

            // Quality score based on returns
            var returns = db.InvoiceReturns
                .Where(ir => purchases.Any(p => p.Guid == ir.OriginalInvoiceGuid))
                .ToList();

            var qualityScore = 100m;
            if (totalOrders > 0)
            {
                qualityScore = 100 - (returns.Sum(r => (int)r.Quantity) / (decimal)totalOrders * 5);
            }

            var onTimePercentage = 85m; // Default - can be enhanced with delivery date tracking
            var overallRating = (onTimePercentage + qualityScore) / 2;

            performanceList.Add(new SupplierPerformance
            {
                SupplierGuid = supplier.Guid,
                SupplierName = supplier.Name,
                TotalPurchases = totalPurchases,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                OnTimePercentage = onTimePercentage,
                QualityScore = qualityScore,
                OverallRating = overallRating,
                LastPurchaseDate = lastPurchaseDate
            });
        }

        return performanceList.OrderByDescending(s => s.OverallRating).ToList();
    }

    // ==================== Supplier Contact Management ====================

    public class SupplierContact
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public List<SupplierContact> GetSupplierContacts(Guid supplierGuid)
    {
        using var db = new LocalDbContext();

        var supplier = db.Suppliers.FirstOrDefault(s => s.Guid == supplierGuid && !s.IsDeleted);
        if (supplier == null) return new List<SupplierContact>();

        return new List<SupplierContact>
        {
            new()
            {
                Name = supplier.Name,
                Phone = supplier.Phone,
                Email = "",
                Department = "Sales"
            }
        };
    }

    // ==================== Supplier Comparison ====================

    public class SupplierComparison
    {
        public string SupplierName { get; set; } = string.Empty;
        public decimal AverageUnitPrice { get; set; }
        public int TotalProductsSupplied { get; set; }
        public decimal PriceCompetitiveness { get; set; }
    }

    public List<SupplierComparison> CompareSupplierPrices(Guid productGuid)
    {
        using var db = new LocalDbContext();

        var purchaseItems = db.PurchaseInvoices
            .Include(pi => pi.Items)
            .Where(pi => !pi.IsDeleted)
            .SelectMany(pi => pi.Items.Where(pii => pii.ProductGuid == productGuid).Select(item => new { pi.SupplierGuid, item.CostPrice }))
            .ToList();

        var comparisons = new Dictionary<Guid, List<decimal>>();

        foreach (var purchase in purchaseItems)
        {
            if (!comparisons.ContainsKey(purchase.SupplierGuid))
                comparisons[purchase.SupplierGuid] = new List<decimal>();
            comparisons[purchase.SupplierGuid].Add(purchase.CostPrice);
        }

        var result = new List<SupplierComparison>();
        var averagePrices = comparisons.Values.SelectMany(x => x).ToList();
        var avgPrice = averagePrices.Count > 0 ? averagePrices.Average() : 0;

        foreach (var comparison in comparisons)
        {
            var supplier = db.Suppliers.FirstOrDefault(s => s.Guid == comparison.Key);
            if (supplier == null) continue;

            var avgUnitPrice = comparison.Value.Average();
            var priceCompetitiveness = avgPrice > 0 ? ((avgPrice - avgUnitPrice) / avgPrice) * 100 : 0;

            result.Add(new SupplierComparison
            {
                SupplierName = supplier.Name,
                AverageUnitPrice = avgUnitPrice,
                TotalProductsSupplied = comparison.Value.Count,
                PriceCompetitiveness = priceCompetitiveness
            });
        }

        return result.OrderByDescending(s => s.PriceCompetitiveness).ToList();
    }

    // ==================== Payment Terms ====================

    public class PaymentTermsInfo
    {
        public string TermsCode { get; set; } = string.Empty;
        public int DaysAllowed { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public List<PaymentTermsInfo> GetStandardPaymentTerms()
    {
        return new List<PaymentTermsInfo>
        {
            new() { TermsCode = "COD", DaysAllowed = 0, DiscountPercentage = 0, Description = "Cash on Delivery" },
            new() { TermsCode = "NET15", DaysAllowed = 15, DiscountPercentage = 2, Description = "Net 15 (2% if paid in 7 days)" },
            new() { TermsCode = "NET30", DaysAllowed = 30, DiscountPercentage = 1.5m, Description = "Net 30 (1.5% if paid in 10 days)" },
            new() { TermsCode = "NET60", DaysAllowed = 60, DiscountPercentage = 1, Description = "Net 60 (1% if paid in 15 days)" }
        };
    }

    // ==================== Outstanding Payables ====================

    public class OutstandingPayable
    {
        public string SupplierName { get; set; } = string.Empty;
        public string PurchaseNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Outstanding { get; set; }
        public int DaysOverdue { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public List<OutstandingPayable> GetOutstandingPayables()
    {
        using var db = new LocalDbContext();

        var payables = new List<OutstandingPayable>();
        var invoices = db.PurchaseInvoices
            .Where(pi => pi.Status != "Paid" && !pi.IsDeleted)
            .ToList();

        var today = DateTime.Now;

        foreach (var invoice in invoices)
        {
            var supplier = db.Suppliers.FirstOrDefault(s => s.Guid == invoice.SupplierGuid);
            var daysOverdue = (int)(today - invoice.PurchaseDate).TotalDays;
            var outstanding = invoice.SubTotal - invoice.PaidAmount;

            var status = daysOverdue switch
            {
                <= 30 => "Current",
                <= 60 => "30-60 Days Overdue",
                <= 90 => "60-90 Days Overdue",
                _ => "90+ Days Overdue"
            };

            payables.Add(new OutstandingPayable
            {
                SupplierName = supplier?.Name ?? "Unknown",
                PurchaseNumber = invoice.PurchaseNumber,
                PurchaseDate = invoice.PurchaseDate,
                Amount = invoice.SubTotal,
                Outstanding = outstanding,
                DaysOverdue = daysOverdue,
                Status = status
            });
        }

        return payables.OrderByDescending(p => p.DaysOverdue).ToList();
    }
}
