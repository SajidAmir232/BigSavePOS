using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

/// <summary>
/// Comprehensive inventory management service with returns, adjustments, and tracking
/// </summary>
public class InventoryManagementService
{
    // ==================== Stock Level Monitoring ====================

    public class StockAlert
    {
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public string AlertType { get; set; } = string.Empty; // "Low Stock", "Out of Stock", "Overstock"
        public DateTime AlertDate { get; set; }
    }

    public List<StockAlert> GetStockAlerts()
    {
        using var db = new LocalDbContext();

        var alerts = new List<StockAlert>();
        var products = db.Products.Where(p => !p.IsDeleted).ToList();

        foreach (var product in products)
        {
            var reorderLevel = 10; // Default, can be made configurable per product
            var alertType = "";

            if (product.Quantity == 0)
            {
                alertType = "Out of Stock";
            }
            else if (product.Quantity < reorderLevel)
            {
                alertType = "Low Stock";
            }
            else if (product.Quantity > 100)
            {
                alertType = "Overstock";
            }

            if (!string.IsNullOrEmpty(alertType))
            {
                alerts.Add(new StockAlert
                {
                    ProductName = product.Name,
                    Sku = product.Sku,
                    CurrentQuantity = product.Quantity,
                    ReorderLevel = reorderLevel,
                    AlertType = alertType,
                    AlertDate = DateTime.Now
                });
            }
        }

        return alerts.OrderByDescending(a => a.AlertDate).ToList();
    }

    // ==================== Returns Processing ====================

    public class ReturnProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public int UpdatedQuantity { get; set; }
    }

    public ReturnProcessingResult ProcessReturn(Guid invoiceGuid, Guid productGuid, int quantity, string reason)
    {
        using var db = new LocalDbContext();
        using var transaction = db.Database.BeginTransaction();

        try
        {
            var invoice = db.Invoices.Include(i => i.Items).FirstOrDefault(i => i.Guid == invoiceGuid);
            if (invoice == null)
                return new ReturnProcessingResult { Success = false, Message = "Invoice not found" };

            var invoiceItem = invoice.Items.FirstOrDefault(i => i.ProductGuid == productGuid);
            if (invoiceItem == null)
                return new ReturnProcessingResult { Success = false, Message = "Product not found in invoice" };

            var product = db.Products.FirstOrDefault(p => p.Guid == productGuid);
            if (product == null)
                return new ReturnProcessingResult { Success = false, Message = "Product not found" };

            // Calculate refund amount
            var refundAmount = quantity * invoiceItem.UnitPrice;

            // Create return record
            var invoiceReturn = new InvoiceReturn
            {
                Guid = Guid.NewGuid(),
                OriginalInvoiceGuid = invoiceGuid,
                ProductGuid = productGuid,
                IMEI = invoiceItem.IMEI,
                Quantity = quantity,
                Reason = reason,
                ReturnDate = DateTime.Now,
                UpdatedAtUtc = DateTime.UtcNow
            };

            db.InvoiceReturns.Add(invoiceReturn);

            // Update product quantity
            product.Quantity += quantity;
            product.UpdatedAtUtc = DateTime.UtcNow;

            // Update invoice
            invoice.PaidAmount -= refundAmount;
            invoice.GrandTotal -= refundAmount;
            invoice.UpdatedAtUtc = DateTime.UtcNow;

            // Update customer credit
            var customer = db.Customers.FirstOrDefault(c => c.Guid == invoice.CustomerGuid);
            if (customer != null)
            {
                customer.CurrentBalance -= refundAmount;
                customer.UpdatedAtUtc = DateTime.UtcNow;
            }

            db.SaveChanges();
            transaction.Commit();

            return new ReturnProcessingResult
            {
                Success = true,
                Message = "Return processed successfully",
                RefundAmount = refundAmount,
                UpdatedQuantity = product.Quantity
            };
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return new ReturnProcessingResult { Success = false, Message = $"Error processing return: {ex.Message}" };
        }
    }

    // ==================== Inventory Aging ====================

    public class AgedInventory
    {
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public int DaysInStock { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public List<AgedInventory> GetAgedInventory()
    {
        using var db = new LocalDbContext();

        var products = db.Products.Where(p => !p.IsDeleted && p.Quantity > 0).ToList();
        var agedItems = new List<AgedInventory>();

        var today = DateTime.Now;

        foreach (var product in products)
        {
            // Get last purchase date
            var lastPurchaseDate = db.PurchaseInvoices
                .Include(pi => pi.Items)
                .Where(pi => !pi.IsDeleted)
                .SelectMany(pi => pi.Items)
                .Where(item => item.ProductGuid == product.Guid)
                .OrderByDescending(item => item.PurchaseInvoiceGuid)
                .FirstOrDefault()
                ?.PurchaseInvoiceGuid ?? Guid.Empty;

            var daysInStock = 0;
            if (lastPurchaseDate != Guid.Empty)
            {
                var purchase = db.PurchaseInvoices.FirstOrDefault(pi => pi.Guid == lastPurchaseDate);
                if (purchase != null)
                {
                    daysInStock = (int)(today - purchase.PurchaseDate).TotalDays;
                }
            }

            agedItems.Add(new AgedInventory
            {
                ProductName = product.Name,
                Sku = product.Sku,
                Quantity = product.Quantity,
                Value = product.Quantity * product.SalePrice,
                DaysInStock = daysInStock,
                Category = product.Category ?? "General"
            });
        }

        return agedItems.OrderByDescending(a => a.DaysInStock).ToList();
    }

    // ==================== Return History ====================

    public class ReturnHistory
    {
        public DateTime ReturnDate { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
    }

    public List<ReturnHistory> GetReturnHistory(DateTime fromDate, DateTime toDate)
    {
        using var db = new LocalDbContext();

        var returns = db.InvoiceReturns
            .Where(ir => ir.ReturnDate >= fromDate && ir.ReturnDate <= toDate && !ir.IsDeleted)
            .OrderByDescending(ir => ir.ReturnDate)
            .ToList();

        var history = new List<ReturnHistory>();

        foreach (var ret in returns)
        {
            var invoice = db.Invoices.FirstOrDefault(i => i.Guid == ret.OriginalInvoiceGuid);
            var customer = invoice != null ? db.Customers.FirstOrDefault(c => c.Guid == invoice.CustomerGuid) : null;
            var refundAmount = ret.Quantity * (invoice?.Items.FirstOrDefault(i => i.ProductGuid == ret.ProductGuid)?.UnitPrice ?? 0);
            var product = db.Products.FirstOrDefault(p => p.Guid == ret.ProductGuid);

            history.Add(new ReturnHistory
            {
                ReturnDate = ret.ReturnDate,
                InvoiceNumber = invoice?.InvoiceNumber ?? "N/A",
                ProductName = product?.Name ?? "Unknown",
                Quantity = ret.Quantity,
                RefundAmount = refundAmount,
                Reason = ret.Reason ?? "Not specified",
                CustomerName = customer?.Name ?? "Unknown"
            });
        }

        return history;
    }
}
