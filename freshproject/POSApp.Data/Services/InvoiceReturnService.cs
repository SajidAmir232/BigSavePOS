using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class InvoiceReturnService
{
    private readonly ProductService _productService = new();

    public List<InvoiceReturn> GetAll()
    {
        using var db = new LocalDbContext();
        return db.InvoiceReturns.Where(ir => !ir.IsDeleted).OrderByDescending(ir => ir.ReturnDate).ToList();
    }

    public List<InvoiceReturn> GetByInvoiceGuid(Guid invoiceGuid)
    {
        using var db = new LocalDbContext();
        return db.InvoiceReturns.Where(ir => ir.OriginalInvoiceGuid == invoiceGuid && !ir.IsDeleted).ToList();
    }

    public InvoiceReturn Save(InvoiceReturn invoiceReturn)
    {
        using var db = new LocalDbContext();
        invoiceReturn.UpdatedAtUtc = DateTime.UtcNow;
        invoiceReturn.IsSynced = false;

        if (invoiceReturn.Id == 0)
            db.InvoiceReturns.Add(invoiceReturn);
        else
            db.InvoiceReturns.Update(invoiceReturn);

        db.SaveChanges();
        return invoiceReturn;
    }

    public void ProcessReturn(InvoiceReturn invoiceReturn)
    {
        using var db = new LocalDbContext();
        using var tx = db.Database.BeginTransaction();

        // Save the return record
        invoiceReturn.UpdatedAtUtc = DateTime.UtcNow;
        invoiceReturn.IsSynced = false;

        if (invoiceReturn.Id == 0)
            db.InvoiceReturns.Add(invoiceReturn);
        else
            db.InvoiceReturns.Update(invoiceReturn);

        db.SaveChanges();

        // Restore stock (add back the returned quantity) - using same db context
        var product = db.Products.FirstOrDefault(p => p.Guid == invoiceReturn.ProductGuid);
        if (product != null)
        {
            product.Quantity += (int)invoiceReturn.Quantity;
            product.IsSynced = false;
            product.UpdatedAtUtc = DateTime.UtcNow;
        }

        // Update invoice status
        var invoice = db.Invoices.FirstOrDefault(i => i.Guid == invoiceReturn.OriginalInvoiceGuid);
        if (invoice != null)
        {
            var totalReturned = db.InvoiceReturns
                .Where(ir => ir.OriginalInvoiceGuid == invoiceReturn.OriginalInvoiceGuid && !ir.IsDeleted)
                .ToList()
                .Sum(ir => ir.Quantity);

            var totalOriginal = invoice.Items.Sum(i => i.Quantity);

            if (totalReturned >= totalOriginal)
                invoice.Status = "Returned";
            else
                invoice.Status = "Partially Returned";

            invoice.IsSynced = false;
            invoice.UpdatedAtUtc = DateTime.UtcNow;
        }

        db.SaveChanges();
        tx.Commit();
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var invoiceReturn = db.InvoiceReturns.Find(id);
        if (invoiceReturn == null) return;

        invoiceReturn.IsDeleted = true;
        invoiceReturn.IsSynced = false;
        invoiceReturn.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }
}
