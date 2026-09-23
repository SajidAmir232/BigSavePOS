using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class InvoiceService
{
    public List<Invoice> GetAll(User? actor = null)
    {
        using var db = new LocalDbContext(); var q = db.Invoices.Include(i => i.Items).Where(i => !i.IsDeleted);
        if (actor != null && !PosAuthorizationService.IsAdmin(actor)) q = q.Where(i => i.BranchId == (actor.PrimaryBranchId ?? 1));
        return q.OrderByDescending(i => i.InvoiceDate).ToList();
    }

    public Invoice Create(Invoice invoice, string deviceId, User? actor = null, IEnumerable<SalePayment>? payments = null)
    {
        if (actor != null) PosAuthorizationService.Require(actor.IsActive, "Inactive user cannot create sales.");
        using var db = new LocalDbContext(); using var tx = db.Database.BeginTransaction();
        invoice.BranchId = actor?.PrimaryBranchId ?? invoice.BranchId; if (invoice.BranchId <= 0) invoice.BranchId = 1;
        invoice.InvoiceNumber = GenerateInvoiceNumber(db, invoice.BranchId); invoice.CreatedByDeviceId = deviceId; invoice.UpdatedAtUtc = DateTime.UtcNow; invoice.IsSynced = false;
        var items = invoice.Items.ToList();
        invoice.Items.Clear();
        foreach (var item in items)
        {
            var product = db.Products.FirstOrDefault(p => p.Guid == item.ProductGuid && !p.IsDeleted && p.BranchId == invoice.BranchId);
            if (product == null) throw new InvalidOperationException($"Product not found: {item.ProductName}");
            if (item.Quantity <= 0 || product.Quantity < item.Quantity) throw new InvalidOperationException($"Stock insufficient for {product.Name}. Available: {product.Quantity}");
        }
        db.Invoices.Add(invoice); db.SaveChanges();
        foreach (var item in items)
        {
            item.InvoiceGuid = invoice.Guid;
            db.InvoiceItems.Add(item);
            db.Entry(item).Property("InvoiceId").CurrentValue = invoice.Id;
            var product = db.Products.First(p => p.Guid == item.ProductGuid); product.Quantity -= (int)item.Quantity; product.UpdatedAtUtc = DateTime.UtcNow; product.IsSynced = false;
            db.InventoryTransactions.Add(new InventoryTransaction { ProductGuid = product.Guid, BranchId = invoice.BranchId, QuantityChange = -item.Quantity, QuantityAfter = product.Quantity, Type = "Sale", Reference = invoice.InvoiceNumber, CreatedBy = actor?.Username });
        }
        if (payments != null) db.SalePayments.AddRange(payments.Select(p => { p.InvoiceGuid = invoice.Guid; return p; }));
        db.SaveChanges(); tx.Commit(); return invoice;
    }

    private string GenerateInvoiceNumber(LocalDbContext db, int branchId)
    {
        var prefix = $"INV-{DateTime.Now:yyyyMMdd}-{branchId}-"; var count = db.Invoices.Count(i => i.InvoiceNumber.StartsWith(prefix)) + 1; return $"{prefix}{count:D4}";
    }
}
