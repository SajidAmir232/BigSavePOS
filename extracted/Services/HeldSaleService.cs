using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public sealed class HeldSaleService
{
    public HeldSale Save(HeldSale sale)
    {
        using var db = new LocalDbContext();
        sale.HeldAtUtc = DateTime.UtcNow;
        db.HeldSales.Add(sale);
        db.SaveChanges();
        return sale;
    }

    public List<HeldSale> GetOpen(int branchId)
    {
        using var db = new LocalDbContext();
        return db.HeldSales.Include(x => x.Items).Where(x => x.BranchId == branchId).OrderByDescending(x => x.HeldAtUtc).ToList();
    }

    public HeldSale? Take(Guid guid)
    {
        using var db = new LocalDbContext();
        var sale = db.HeldSales.Include(x => x.Items).FirstOrDefault(x => x.Guid == guid);
        if (sale != null) { db.HeldSales.Remove(sale); db.SaveChanges(); }
        return sale;
    }

    public void Delete(Guid guid)
    {
        using var db = new LocalDbContext();
        var sale = db.HeldSales.FirstOrDefault(x => x.Guid == guid);
        if (sale != null) { db.HeldSales.Remove(sale); db.SaveChanges(); }
    }
}
