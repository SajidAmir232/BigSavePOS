using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services
{
    public class CustomerPaymentService
    {
        public void AddPayment(CustomerPayment payment)
        {
            using var db = new LocalDbContext();
            payment.IsSynced = false;
            db.CustomerPayments.Add(payment);
            db.SaveChanges();
        }

        public List<CustomerPayment> GetPaymentsByCustomer(Guid customerGuid)
        {
            using var db = new LocalDbContext();
            return db.CustomerPayments
                .Where(cp => cp.CustomerGuid == customerGuid && !cp.IsDeleted)
                .OrderByDescending(cp => cp.PaymentDate)
                .ToList();
        }

        public decimal GetTotalPaymentsByCustomer(Guid customerGuid)
        {
            using var db = new LocalDbContext();
            var payments = db.CustomerPayments
                .Where(cp => cp.CustomerGuid == customerGuid && !cp.IsDeleted)
                .ToList();
            return payments.Sum(cp => cp.Amount);
        }

        public void DeletePayment(int paymentId)
        {
            using var db = new LocalDbContext();
            var payment = db.CustomerPayments.FirstOrDefault(cp => cp.Id == paymentId);
            if (payment != null)
            {
                payment.IsDeleted = true;
                payment.IsSynced = false;
                db.SaveChanges();
            }
        }

        public List<CustomerPayment> GetAllPayments()
        {
            using var db = new LocalDbContext();
            return db.CustomerPayments.Where(cp => !cp.IsDeleted).ToList();
        }
    }
}
