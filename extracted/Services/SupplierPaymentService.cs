using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class SupplierPaymentService
{
    public List<SupplierPayment> GetAll()
    {
        using var db = new LocalDbContext();
        return db.SupplierPayments.Where(sp => !sp.IsDeleted).OrderByDescending(sp => sp.PaymentDate).ToList();
    }

    public List<SupplierPayment> GetBySupplierGuid(Guid supplierGuid)
    {
        using var db = new LocalDbContext();
        return db.SupplierPayments.Where(sp => sp.SupplierGuid == supplierGuid && !sp.IsDeleted).OrderByDescending(sp => sp.PaymentDate).ToList();
    }

    public SupplierPayment Save(SupplierPayment payment)
    {
        using var db = new LocalDbContext();
        payment.UpdatedAtUtc = DateTime.UtcNow;
        payment.IsSynced = false;

        if (payment.Id == 0)
            db.SupplierPayments.Add(payment);
        else
            db.SupplierPayments.Update(payment);

        db.SaveChanges();
        return payment;
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var payment = db.SupplierPayments.Find(id);
        if (payment == null) return;

        payment.IsDeleted = true;
        payment.IsSynced = false;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    public decimal GetTotalPaidAmount(Guid supplierGuid)
    {
        using var db = new LocalDbContext();
        var payments = db.SupplierPayments
            .Where(sp => sp.SupplierGuid == supplierGuid && !sp.IsDeleted)
            .ToList();
        return payments.Sum(sp => sp.Amount);
    }
}
