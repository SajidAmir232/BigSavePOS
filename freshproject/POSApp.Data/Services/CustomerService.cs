using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class CustomerService
{
    public List<Customer> GetAll()
    {
        using var db = new LocalDbContext();
        return db.Customers.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();
    }

    public Customer? GetByGuid(Guid guid)
    {
        using var db = new LocalDbContext();
        return db.Customers.FirstOrDefault(c => c.Guid == guid && !c.IsDeleted);
    }

    public Customer Save(Customer customer)
    {
        using var db = new LocalDbContext();
        customer.UpdatedAtUtc = DateTime.UtcNow;
        customer.IsSynced = false; // mark dirty so SyncService pushes it next time

        if (customer.Id == 0)
            db.Customers.Add(customer);
        else
            db.Customers.Update(customer);

        db.SaveChanges();
        return customer;
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var customer = db.Customers.Find(id);
        if (customer == null) return;

        // Soft delete -> so the deletion also gets synced instead of just disappearing locally
        customer.IsDeleted = true;
        customer.IsSynced = false;
        customer.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    public decimal GetOutstandingBalance(Guid customerGuid)
    {
        using var db = new LocalDbContext();
        var customer = db.Customers.FirstOrDefault(c => c.Guid == customerGuid);
        if (customer == null) return 0;

        // Opening balance + unpaid invoices - payments
        var unpaidInvoices = db.Invoices
            .Where(i => i.CustomerGuid == customerGuid && i.Status != "Paid" && !i.IsDeleted)
            .ToList();
        var unpaidAmount = unpaidInvoices.Sum(i => i.GrandTotal - i.PaidAmount);

        return customer.OpeningBalance + unpaidAmount;
    }
}
