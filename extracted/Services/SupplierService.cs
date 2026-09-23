using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class SupplierService
{
    public List<Supplier> GetAll()
    {
        using var db = new LocalDbContext();
        return db.Suppliers.Where(s => !s.IsDeleted).OrderBy(s => s.Name).ToList();
    }

    public Supplier? GetByGuid(Guid guid)
    {
        using var db = new LocalDbContext();
        return db.Suppliers.FirstOrDefault(s => s.Guid == guid && !s.IsDeleted);
    }

    public Supplier Save(Supplier supplier)
    {
        using var db = new LocalDbContext();
        supplier.UpdatedAtUtc = DateTime.UtcNow;
        supplier.IsSynced = false;

        if (supplier.Id == 0)
            db.Suppliers.Add(supplier);
        else
            db.Suppliers.Update(supplier);

        db.SaveChanges();
        return supplier;
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var supplier = db.Suppliers.Find(id);
        if (supplier == null) return;

        supplier.IsDeleted = true;
        supplier.IsSynced = false;
        supplier.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    public decimal GetPayableBalance(Guid supplierGuid)
    {
        using var db = new LocalDbContext();
        var supplier = db.Suppliers.FirstOrDefault(s => s.Guid == supplierGuid);
        if (supplier == null) return 0;

        // Opening balance + unpaid purchase invoices - payments
        var unpaidInvoices = db.PurchaseInvoices
            .Where(pi => pi.SupplierGuid == supplierGuid && pi.Status != "Paid" && !pi.IsDeleted)
            .ToList();
        var unpaidAmount = unpaidInvoices.Sum(pi => pi.SubTotal - pi.PaidAmount);

        return supplier.OpeningBalance + unpaidAmount;
    }
}
