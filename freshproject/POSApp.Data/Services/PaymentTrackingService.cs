using POSApp.Data.Models;

namespace POSApp.Data.Services;

/// <summary>
/// Service for managing card payment tracking
/// </summary>
public class PaymentTrackingService
{
    public class CardPaymentRecord
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }

    /// <summary>
    /// Get all card payments for a date range
    /// </summary>
    public List<CardPaymentRecord> GetCardPaymentsByDateRange(DateTime fromDate, DateTime toDate)
    {
        using var db = new LocalDbContext();

        var cardInvoices = db.Invoices
            .Where(i => i.SaleType == "Card" && 
                       i.InvoiceDate.Date >= fromDate.Date && 
                       i.InvoiceDate.Date <= toDate.Date &&
                       !i.IsDeleted)
            .ToList();

        var customers = db.Customers.ToList();

        var cardPayments = cardInvoices.Select(i => new CardPaymentRecord
        {
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            Amount = i.GrandTotal,
            CustomerName = customers.FirstOrDefault(c => c.Guid == i.CustomerGuid)?.Name ?? "Walk-in",
            Status = i.Status
        }).ToList();

        return cardPayments.OrderByDescending(p => p.InvoiceDate).ToList();
    }

    /// <summary>
    /// Get pending card payments (unpaid)
    /// </summary>
    public List<CardPaymentRecord> GetPendingCardPayments()
    {
        using var db = new LocalDbContext();

        var pendingInvoices = db.Invoices
            .Where(i => i.SaleType == "Card" && i.Status != "Paid" && !i.IsDeleted)
            .ToList();

        var customers = db.Customers.ToList();

        var pendingPayments = pendingInvoices.Select(i => new CardPaymentRecord
        {
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            Amount = i.GrandTotal,
            CustomerName = customers.FirstOrDefault(c => c.Guid == i.CustomerGuid)?.Name ?? "Walk-in",
            Status = i.Status
        }).ToList();

        return pendingPayments.OrderByDescending(p => p.InvoiceDate).ToList();
    }

    /// <summary>
    /// Mark payment status
    /// </summary>
    public bool UpdatePaymentStatus(string invoiceNumber, string newStatus)
    {
        try
        {
            using var db = new LocalDbContext();
            var invoice = db.Invoices.FirstOrDefault(i => i.InvoiceNumber == invoiceNumber);

            if (invoice == null || invoice.SaleType != "Card")
                return false;

            invoice.Status = newStatus;
            db.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating payment status: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get card payment summary
    /// </summary>
    public (decimal TotalAmount, int Count, int PaidCount) GetCardPaymentSummary(DateTime fromDate, DateTime toDate)
    {
        using var db = new LocalDbContext();

        var cardInvoices = db.Invoices
            .Where(i => i.SaleType == "Card" && 
                       i.InvoiceDate.Date >= fromDate.Date && 
                       i.InvoiceDate.Date <= toDate.Date &&
                       !i.IsDeleted)
            .ToList();

        var totalAmount = cardInvoices.Sum(i => i.GrandTotal);
        var count = cardInvoices.Count;
        var paidCount = cardInvoices.Count(i => i.Status == "Paid");

        return (totalAmount, count, paidCount);
    }
}
