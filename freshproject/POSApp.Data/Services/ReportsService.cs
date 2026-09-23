using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services
{
    /// <summary>
    /// Comprehensive reporting service for all reports
    /// </summary>
    public class ReportsService
    {
        #region Sales Summary Report

        public class SalesSummaryDto
        {
            public string Metric { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public List<SalesSummaryDto> GetSalesSummaryReport(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var invoices = db.Invoices
                .Where(i => i.InvoiceDate.Date >= fromDate.Date && i.InvoiceDate.Date <= toDate.Date && !i.IsDeleted)
                .Include(i => i.Items)
                .ToList();

            var report = new List<SalesSummaryDto>
            {
                new() { Metric = "Total Invoices", Value = invoices.Count.ToString() },
                new() { Metric = "Total Sales Amount", Value = "Rs" + invoices.Sum(i => i.GrandTotal).ToString("N0") },
                new() { Metric = "Average Sale Amount", Value = "Rs" + (invoices.Any() ? invoices.Average(i => i.GrandTotal) : 0).ToString("N0") },
                new() { Metric = "Total Items Sold", Value = invoices.SelectMany(i => i.Items).Sum(x => (int)x.Quantity).ToString() },
                new() { Metric = "Total Discount", Value = "Rs" + invoices.Sum(i => i.Discount).ToString("N0") },
                new() { Metric = "Total Tax", Value = "Rs" + invoices.Sum(i => i.Tax).ToString("N0") }
            };

            return report;
        }

        #endregion

        #region Sales by Cashier Report

        public class SalesByCashierDto
        {
            public string CashierName { get; set; } = string.Empty;
            public string InvoiceCount { get; set; } = string.Empty;
            public string TotalSales { get; set; } = string.Empty;
        }

        public List<SalesByCashierDto> GetSalesByCashierReport(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var invoices = db.Invoices
                .Where(i => i.InvoiceDate.Date >= fromDate.Date && i.InvoiceDate.Date <= toDate.Date && !i.IsDeleted)
                .ToList();

            var users = db.Users.Where(u => !u.IsDeleted).ToList();

            var report = new List<SalesByCashierDto>();

            var groupedByCreatedBy = invoices.GroupBy(i => i.CreatedByDeviceId);

            foreach (var group in groupedByCreatedBy)
            {
                var user = users.FirstOrDefault(u => u.Username == group.Key) ?? new User { Username = group.Key };

                report.Add(new SalesByCashierDto
                {
                    CashierName = user.Username,
                    InvoiceCount = group.Count().ToString(),
                    TotalSales = "Rs" + group.Sum(i => i.GrandTotal).ToString("N0")
                });
            }

            return report.OrderByDescending(r => decimal.Parse(r.TotalSales.Replace("Rs", "").Replace(",", ""))).ToList();
        }

        #endregion

        #region Sales by Product/Category Report

        public class SalesByProductDto
        {
            public string ProductName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Quantity { get; set; } = string.Empty;
            public string TotalSales { get; set; } = string.Empty;
        }

        public List<SalesByProductDto> GetSalesByProductReport(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var invoices = db.Invoices
                .Where(i => i.InvoiceDate.Date >= fromDate.Date && i.InvoiceDate.Date <= toDate.Date && !i.IsDeleted)
                .Include(i => i.Items)
                .ToList();

            var products = db.Products.ToList();

            var report = new List<SalesByProductDto>();

            var invoiceItems = invoices.SelectMany(i => i.Items).ToList();
            var groupedByProduct = invoiceItems.GroupBy(x => x.ProductGuid);

            foreach (var group in groupedByProduct)
            {
                var product = products.FirstOrDefault(p => p.Guid == group.Key) ?? new Product { Name = "Unknown" };

                report.Add(new SalesByProductDto
                {
                    ProductName = product.Name,
                    Category = product.Category ?? "Uncategorized",
                    Quantity = group.Sum(x => (int)x.Quantity).ToString(),
                    TotalSales = "Rs" + group.Sum(x => x.Total).ToString("N0")
                });
            }

            return report.OrderByDescending(r => decimal.Parse(r.TotalSales.Replace("Rs", "").Replace(",", ""))).Take(20).ToList();
        }

        #endregion

        #region Low Stock / Out of Stock Report

        public class LowStockDto
        {
            public string ProductName { get; set; } = string.Empty;
            public string Sku { get; set; } = string.Empty;
            public string CurrentStock { get; set; } = string.Empty;
            public string LowStockThreshold { get; set; } = "10";
            public string Status { get; set; } = string.Empty;
        }

        public List<LowStockDto> GetLowStockReport()
        {
            using var db = new LocalDbContext();

            var products = db.Products
                .Where(p => !p.IsDeleted && p.Quantity <= 10)
                .OrderBy(p => p.Quantity)
                .ToList();

            var report = products.Select(p => new LowStockDto
            {
                ProductName = p.Name,
                Sku = p.Sku ?? "N/A",
                CurrentStock = p.Quantity.ToString(),
                Status = p.Quantity == 0 ? "Out of Stock" : "Low Stock"
            }).ToList();

            return report;
        }

        #endregion

        #region Dead Stock Report

        public class DeadStockDto
        {
            public string ProductName { get; set; } = string.Empty;
            public string Sku { get; set; } = string.Empty;
            public string CurrentStock { get; set; } = string.Empty;
            public string LastSaleDate { get; set; } = string.Empty;
            public string DaysSinceLastSale { get; set; } = string.Empty;
        }

        public List<DeadStockDto> GetDeadStockReport(int daysThreshold = 30)
        {
            using var db = new LocalDbContext();

            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            var invoices = db.Invoices.Where(i => !i.IsDeleted).Include(i => i.Items).ToList();

            var report = new List<DeadStockDto>();
            var cutoffDate = DateTime.Now.AddDays(-daysThreshold);

            foreach (var product in products)
            {
                var lastSale = invoices
                    .SelectMany(i => i.Items)
                    .Where(x => x.ProductGuid == product.Guid)
                    .OrderByDescending(x => x.InvoiceGuid)
                    .Select(x => invoices.FirstOrDefault(i => i.Guid == x.InvoiceGuid)?.InvoiceDate)
                    .FirstOrDefault();

                if (lastSale == null || lastSale < cutoffDate)
                {
                    report.Add(new DeadStockDto
                    {
                        ProductName = product.Name,
                        Sku = product.Sku ?? "N/A",
                        CurrentStock = product.Quantity.ToString(),
                        LastSaleDate = lastSale?.ToString("yyyy-MM-dd") ?? "Never",
                        DaysSinceLastSale = lastSale != null ? ((int)(DateTime.Now - lastSale.Value).TotalDays).ToString() : "N/A"
                    });
                }
            }

            return report.OrderByDescending(r => string.IsNullOrEmpty(r.DaysSinceLastSale) ? int.MaxValue : int.Parse(r.DaysSinceLastSale)).ToList();
        }

        #endregion

        #region Purchase Summary Report

        public class PurchaseSummaryDto
        {
            public string SupplierName { get; set; } = string.Empty;
            public string InvoiceCount { get; set; } = string.Empty;
            public string TotalPurchase { get; set; } = string.Empty;
        }

        public List<PurchaseSummaryDto> GetPurchaseSummaryReport(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var purchaseInvoices = db.PurchaseInvoices
                .Where(pi => pi.PurchaseDate.Date >= fromDate.Date && pi.PurchaseDate.Date <= toDate.Date && !pi.IsDeleted)
                .ToList();

            var suppliers = db.Suppliers.ToList();

            var report = new List<PurchaseSummaryDto>();

            var groupedBySupplier = purchaseInvoices.GroupBy(pi => pi.SupplierGuid);

            foreach (var group in groupedBySupplier)
            {
                var supplier = suppliers.FirstOrDefault(s => s.Guid == group.Key) ?? new Supplier { Name = "Unknown" };

                report.Add(new PurchaseSummaryDto
                {
                    SupplierName = supplier.Name,
                    InvoiceCount = group.Count().ToString(),
                    TotalPurchase = "Rs" + group.Sum(pi => pi.SubTotal).ToString("N0")
                });
            }

            return report.OrderByDescending(r => decimal.Parse(r.TotalPurchase.Replace("Rs", "").Replace(",", ""))).ToList();
        }

        #endregion

        #region Customer-wise Sales Report

        public class CustomerSalesDto
        {
            public string CustomerName { get; set; } = string.Empty;
            public string InvoiceCount { get; set; } = string.Empty;
            public string TotalSales { get; set; } = string.Empty;
        }

        public List<CustomerSalesDto> GetCustomerWiseSalesReport(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var invoices = db.Invoices
                .Where(i => i.InvoiceDate.Date >= fromDate.Date && i.InvoiceDate.Date <= toDate.Date && !i.IsDeleted)
                .ToList();

            var customers = db.Customers.ToList();

            var report = new List<CustomerSalesDto>();

            var groupedByCustomer = invoices.GroupBy(i => i.CustomerGuid);

            foreach (var group in groupedByCustomer)
            {
                var customer = customers.FirstOrDefault(c => c.Guid == group.Key) ?? new Customer { Name = "Walk-in Customer" };

                report.Add(new CustomerSalesDto
                {
                    CustomerName = customer.Name,
                    InvoiceCount = group.Count().ToString(),
                    TotalSales = "Rs" + group.Sum(i => i.GrandTotal).ToString("N0")
                });
            }

            return report.OrderByDescending(r => decimal.Parse(r.TotalSales.Replace("Rs", "").Replace(",", ""))).Take(20).ToList();
        }

        #endregion

        #region Receivables/Payables Summary Report

        public class ReceivablesPayablesSummaryDto
        {
            public string Type { get; set; } = string.Empty;
            public string TotalAmount { get; set; } = string.Empty;
            public string Current { get; set; } = string.Empty;
            public string DaysRange31To60 { get; set; } = string.Empty;
            public string DaysRange61Plus { get; set; } = string.Empty;
        }

        public List<ReceivablesPayablesSummaryDto> GetReceivablesPayablesSummaryReport()
        {
            using var db = new LocalDbContext();

            var today = DateTime.Now;
            var report = new List<ReceivablesPayablesSummaryDto>();

            // Receivables (Customer Invoices)
            var creditInvoices = db.Invoices
                .Where(i => i.SaleType == "Credit" && i.Status != "Paid" && !i.IsDeleted)
                .ToList();

            var receivablesTotal = creditInvoices.Sum(i => i.GrandTotal - i.PaidAmount);
            var receivablesCurrent = creditInvoices.Where(i => (today - i.InvoiceDate).TotalDays <= 30).Sum(i => i.GrandTotal - i.PaidAmount);
            var receivables31To60 = creditInvoices.Where(i => (today - i.InvoiceDate).TotalDays > 30 && (today - i.InvoiceDate).TotalDays <= 60).Sum(i => i.GrandTotal - i.PaidAmount);
            var receivables61Plus = creditInvoices.Where(i => (today - i.InvoiceDate).TotalDays > 60).Sum(i => i.GrandTotal - i.PaidAmount);

            report.Add(new ReceivablesPayablesSummaryDto
            {
                Type = "Receivables (Customer)",
                TotalAmount = "Rs" + receivablesTotal.ToString("N0"),
                Current = "Rs" + receivablesCurrent.ToString("N0"),
                DaysRange31To60 = "Rs" + receivables31To60.ToString("N0"),
                DaysRange61Plus = "Rs" + receivables61Plus.ToString("N0")
            });

            // Payables (Supplier Invoices)
            var purchaseInvoices = db.PurchaseInvoices
                .Where(pi => pi.Status != "Paid" && !pi.IsDeleted)
                .ToList();

            var payablesTotal = purchaseInvoices.Sum(pi => pi.SubTotal - pi.PaidAmount);
            var payablesCurrent = purchaseInvoices.Where(pi => (today - pi.PurchaseDate).TotalDays <= 30).Sum(pi => pi.SubTotal - pi.PaidAmount);
            var payables31To60 = purchaseInvoices.Where(pi => (today - pi.PurchaseDate).TotalDays > 30 && (today - pi.PurchaseDate).TotalDays <= 60).Sum(pi => pi.SubTotal - pi.PaidAmount);
            var payables61Plus = purchaseInvoices.Where(pi => (today - pi.PurchaseDate).TotalDays > 60).Sum(pi => pi.SubTotal - pi.PaidAmount);

            report.Add(new ReceivablesPayablesSummaryDto
            {
                Type = "Payables (Supplier)",
                TotalAmount = "Rs" + payablesTotal.ToString("N0"),
                Current = "Rs" + payablesCurrent.ToString("N0"),
                DaysRange31To60 = "Rs" + payables31To60.ToString("N0"),
                DaysRange61Plus = "Rs" + payables61Plus.ToString("N0")
            });

            return report;
        }

        #endregion

        #region Sales Return Report

        public class SalesReturnDto
        {
            public string InvoiceNumber { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public string Quantity { get; set; } = string.Empty;
            public string ReturnReason { get; set; } = string.Empty;
            public string ReturnDate { get; set; } = string.Empty;
        }

        public List<SalesReturnDto> GetSalesReturnReport(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var returns = db.InvoiceReturns
                .Where(ir => ir.ReturnDate.Date >= fromDate.Date && ir.ReturnDate.Date <= toDate.Date && !ir.IsDeleted)
                .ToList();

            var invoices = db.Invoices.ToList();

            var report = returns.Select(r => 
            {
                var originalInvoice = invoices.FirstOrDefault(i => i.Guid == r.OriginalInvoiceGuid);
                var product = db.Products.FirstOrDefault(p => p.Guid == r.ProductGuid);

                return new SalesReturnDto
                {
                    InvoiceNumber = originalInvoice?.InvoiceNumber ?? "Unknown",
                    ProductName = product?.Name ?? "Unknown",
                    Quantity = r.Quantity.ToString(),
                    ReturnReason = r.Reason ?? "Not Specified",
                    ReturnDate = r.ReturnDate.ToString("yyyy-MM-dd")
                };
            }).ToList();

            return report.OrderByDescending(r => r.ReturnDate).ToList();
        }

        #endregion

        #region IMEI Sold Report

        public class IMEISoldDto
        {
            public string IMEI { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string InvoiceNumber { get; set; } = string.Empty;
            public string SaleDate { get; set; } = string.Empty;
            public string SalePrice { get; set; } = string.Empty;
        }

        public List<IMEISoldDto> GetIMEISoldReport(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var invoices = db.Invoices
                .Where(i => i.InvoiceDate.Date >= fromDate.Date && i.InvoiceDate.Date <= toDate.Date && !i.IsDeleted)
                .Include(i => i.Items)
                .ToList();

            var customers = db.Customers.ToList();

            var report = new List<IMEISoldDto>();

            foreach (var invoice in invoices)
            {
                var customer = customers.FirstOrDefault(c => c.Guid == invoice.CustomerGuid) ?? new Customer { Name = "Walk-in" };

                var imeiItems = invoice.Items.Where(x => !string.IsNullOrEmpty(x.IMEI)).ToList();

                foreach (var item in imeiItems)
                {
                    report.Add(new IMEISoldDto
                    {
                        IMEI = item.IMEI,
                        ProductName = item.ProductName,
                        CustomerName = customer.Name,
                        InvoiceNumber = invoice.InvoiceNumber,
                        SaleDate = invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                        SalePrice = "Rs" + item.UnitPrice.ToString("N0")
                    });
                }
            }

            return report.OrderByDescending(r => r.SaleDate).ToList();
        }

        #endregion

        #region Stock Report with Value

        public class StockReportDto
        {
            public string ProductName { get; set; } = string.Empty;
            public string Sku { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string CurrentStock { get; set; } = string.Empty;
            public string PurchasePrice { get; set; } = string.Empty;
            public string StockValue { get; set; } = string.Empty;
        }

        public List<StockReportDto> GetStockReportWithValue()
        {
            using var db = new LocalDbContext();

            var products = db.Products
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Quantity)
                .ToList();

            var report = products.Select(p => new StockReportDto
            {
                ProductName = p.Name,
                Sku = p.Sku ?? "N/A",
                Category = p.Category ?? "Uncategorized",
                CurrentStock = p.Quantity.ToString(),
                PurchasePrice = "Rs" + p.PurchasePrice.ToString("N0"),
                StockValue = "Rs" + (p.Quantity * p.PurchasePrice).ToString("N0")
            }).ToList();

            return report;
        }

        #endregion

        #region Daily Sales Data (for charts)

        public class DailySalesDto
        {
            public DateTime Date { get; set; }
            public decimal SalesAmount { get; set; }
            public decimal PurchaseAmount { get; set; }
        }

        public List<DailySalesDto> GetDailySalesData(DateTime fromDate, DateTime toDate)
        {
            using var db = new LocalDbContext();

            var invoices = db.Invoices
                .Where(i => i.InvoiceDate.Date >= fromDate.Date && i.InvoiceDate.Date <= toDate.Date && !i.IsDeleted)
                .ToList();

            var purchases = db.PurchaseInvoices
                .Where(p => p.PurchaseDate.Date >= fromDate.Date && p.PurchaseDate.Date <= toDate.Date && !p.IsDeleted)
                .ToList();

            var dates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(d => fromDate.AddDays(d))
                .ToList();

            return dates.Select(date => new DailySalesDto
            {
                Date = date,
                SalesAmount = invoices.Where(i => i.InvoiceDate.Date == date.Date).Sum(i => i.GrandTotal),
                PurchaseAmount = purchases.Where(p => p.PurchaseDate.Date == date.Date).Sum(p => p.SubTotal)
            }).ToList();
        }

        #endregion
    }
}
