using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services
{
    public class CustomerLedgerEntry
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "Invoice", "Payment", "Return"
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal Debit { get; set; } // Amount owed (sales)
        public decimal Credit { get; set; } // Amount paid (payments)
        public decimal RunningBalance { get; set; }
    }

    public class OverdueInvoice
    {
        public Invoice Invoice { get; set; } = new();
        public Customer Customer { get; set; } = new();
        public int DaysOverdue { get; set; }
        public decimal AmountDue { get; set; }
    }

    public class CustomerLedgerService
    {
        public List<CustomerLedgerEntry> GetCustomerLedger(Guid customerGuid, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var db = new LocalDbContext();
            var customer = db.Customers.FirstOrDefault(c => c.Guid == customerGuid && !c.IsDeleted);
            if (customer == null) return new List<CustomerLedgerEntry>();

            var ledger = new List<CustomerLedgerEntry>();
            decimal runningBalance = customer.OpeningBalance;

            if (customer.OpeningBalance != 0)
            {
                ledger.Add(new CustomerLedgerEntry
                {
                    Date = DateTime.MinValue,
                    Type = "Opening Balance",
                    ReferenceNumber = "OPENING",
                    Debit = customer.OpeningBalance > 0 ? customer.OpeningBalance : 0,
                    Credit = customer.OpeningBalance < 0 ? Math.Abs(customer.OpeningBalance) : 0,
                    RunningBalance = customer.OpeningBalance
                });
            }

            // Get all credit invoices for this customer
            var creditInvoices = db.Invoices
                .Where(i => i.CustomerGuid == customerGuid && i.SaleType == "Credit" && !i.IsDeleted)
                .OrderBy(i => i.InvoiceDate)
                .ToList();

            foreach (var invoice in creditInvoices)
            {
                if (fromDate.HasValue && invoice.InvoiceDate.Date < fromDate.Value.Date) continue;
                if (toDate.HasValue && invoice.InvoiceDate.Date > toDate.Value.Date) continue;

                var outstanding = invoice.GrandTotal - invoice.PaidAmount;
                if (outstanding > 0)
                {
                    runningBalance += outstanding;
                    ledger.Add(new CustomerLedgerEntry
                    {
                        Date = invoice.InvoiceDate,
                        Type = "Invoice",
                        ReferenceNumber = invoice.InvoiceNumber,
                        Debit = outstanding,
                        Credit = 0,
                        RunningBalance = runningBalance
                    });
                }
            }

            // Get all payments for this customer
            var payments = db.CustomerPayments
                .Where(cp => cp.CustomerGuid == customerGuid && !cp.IsDeleted)
                .OrderBy(cp => cp.PaymentDate)
                .ToList();

            foreach (var payment in payments)
            {
                if (fromDate.HasValue && payment.PaymentDate.Date < fromDate.Value.Date) continue;
                if (toDate.HasValue && payment.PaymentDate.Date > toDate.Value.Date) continue;

                runningBalance -= payment.Amount;
                ledger.Add(new CustomerLedgerEntry
                {
                    Date = payment.PaymentDate,
                    Type = "Payment",
                    ReferenceNumber = $"PMT-{payment.Guid.ToString().Substring(0, 8)}",
                    Debit = 0,
                    Credit = payment.Amount,
                    RunningBalance = runningBalance
                });
            }

            return ledger.OrderBy(l => l.Date).ToList();
        }

        public decimal GetCustomerBalance(Guid customerGuid)
        {
            using var db = new LocalDbContext();
            var customer = db.Customers.FirstOrDefault(c => c.Guid == customerGuid && !c.IsDeleted);
            if (customer == null) return 0;

            decimal balance = customer.OpeningBalance;

            // Add outstanding credit sales
            var creditInvoices = db.Invoices
                .Where(i => i.CustomerGuid == customerGuid && i.SaleType == "Credit" && !i.IsDeleted)
                .ToList();
            var creditSales = creditInvoices.Sum(i => (i.GrandTotal - i.PaidAmount));
            balance += creditSales;

            // Subtract payments
            var paymentsList = db.CustomerPayments
                .Where(cp => cp.CustomerGuid == customerGuid && !cp.IsDeleted)
                .ToList();
            var payments = paymentsList.Sum(cp => cp.Amount);
            balance -= payments;

            return balance;
        }

        public List<OverdueInvoice> GetOverdueInvoices(int daysThreshold = 0)
        {
            using var db = new LocalDbContext();
            var overdue = new List<OverdueInvoice>();

            var creditInvoices = db.Invoices
                .Include(i => i.Items)
                .Where(i => i.SaleType == "Credit" && i.Status != "Paid" && !i.IsDeleted)
                .ToList();

            foreach (var invoice in creditInvoices)
            {
                var daysOverdue = (int)(DateTime.Now - invoice.InvoiceDate).TotalDays;
                if (daysOverdue >= daysThreshold)
                {
                    var customer = db.Customers.FirstOrDefault(c => c.Guid == invoice.CustomerGuid);
                    if (customer != null)
                    {
                        var amountDue = invoice.GrandTotal - invoice.PaidAmount;
                        if (amountDue > 0)
                        {
                            overdue.Add(new OverdueInvoice
                            {
                                Invoice = invoice,
                                Customer = customer,
                                DaysOverdue = daysOverdue,
                                AmountDue = amountDue
                            });
                        }
                    }
                }
            }

            return overdue.OrderByDescending(o => o.DaysOverdue).ToList();
        }

        public (List<OverdueInvoice> ZeroTo30, List<OverdueInvoice> Thirty1To60, List<OverdueInvoice> Over60) GetOverduesByRange()
        {
            var all = GetOverdueInvoices(0);

            var zeroTo30 = all.Where(o => o.DaysOverdue >= 0 && o.DaysOverdue <= 30).ToList();
            var thirty1To60 = all.Where(o => o.DaysOverdue > 30 && o.DaysOverdue <= 60).ToList();
            var over60 = all.Where(o => o.DaysOverdue > 60).ToList();

            return (zeroTo30, thirty1To60, over60);
        }
    }
}
