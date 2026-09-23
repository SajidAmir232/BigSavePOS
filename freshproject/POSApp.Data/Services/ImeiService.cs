using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class ImeiService
{
    public List<ImeiUnit> GetAll()
    {
        using var db = new LocalDbContext();
        return db.ImeiUnits.Where(i => !i.IsDeleted).OrderByDescending(i => i.PurchaseDate).ToList();
    }

    public List<ImeiUnit> GetAvailable()
    {
        using var db = new LocalDbContext();
        return db.ImeiUnits
            .Where(i => !i.IsDeleted && i.Status == "Available")
            .OrderBy(i => i.SerialNumber)
            .ToList();
    }

    public List<ImeiUnit> GetByProductGuid(Guid productGuid)
    {
        using var db = new LocalDbContext();
        return db.ImeiUnits
            .Where(i => i.ProductGuid == productGuid && !i.IsDeleted)
            .OrderBy(i => i.SerialNumber)
            .ToList();
    }

    public ImeiUnit? GetBySerialNumber(string serialNumber)
    {
        using var db = new LocalDbContext();
        return db.ImeiUnits
            .FirstOrDefault(i => i.SerialNumber == serialNumber && !i.IsDeleted);
    }

    public ImeiUnit? GetByGuid(Guid guid)
    {
        using var db = new LocalDbContext();
        return db.ImeiUnits.FirstOrDefault(i => i.Guid == guid && !i.IsDeleted);
    }

    public ImeiUnit Save(ImeiUnit imei)
    {
        using var db = new LocalDbContext();
        imei.UpdatedAtUtc = DateTime.UtcNow;
        imei.IsSynced = false;

        if (imei.Id == 0)
        {
            db.ImeiUnits.Add(imei);
        }
        else
        {
            db.ImeiUnits.Update(imei);
        }

        db.SaveChanges();
        return imei;
    }

    public void MarkAsSold(Guid imeiGuid, Guid invoiceGuid, Guid customerGuid, decimal salePrice)
    {
        using var db = new LocalDbContext();
        var imei = db.ImeiUnits.FirstOrDefault(i => i.Guid == imeiGuid && !i.IsDeleted);
        if (imei == null) throw new Exception("IMEI not found.");

        if (imei.Status == "Sold")
            throw new Exception($"IMEI {imei.SerialNumber} is already sold.");

        imei.Status = "Sold";
        imei.SoldDate = DateTime.Now;
        imei.InvoiceGuid = invoiceGuid;
        imei.CustomerGuid = customerGuid;
        imei.SalePrice = salePrice;
        imei.IsSynced = false;
        imei.UpdatedAtUtc = DateTime.UtcNow;

        db.SaveChanges();
    }

    public void MarkAsReturned(Guid imeiGuid)
    {
        using var db = new LocalDbContext();
        var imei = db.ImeiUnits.FirstOrDefault(i => i.Guid == imeiGuid && !i.IsDeleted);
        if (imei == null) throw new Exception("IMEI not found.");

        imei.Status = "Returned";
        imei.SoldDate = null;
        imei.InvoiceGuid = null;
        imei.CustomerGuid = null;
        imei.IsSynced = false;
        imei.UpdatedAtUtc = DateTime.UtcNow;

        db.SaveChanges();
    }

    public void MarkAsDefective(Guid imeiGuid)
    {
        using var db = new LocalDbContext();
        var imei = db.ImeiUnits.FirstOrDefault(i => i.Guid == imeiGuid && !i.IsDeleted);
        if (imei == null) throw new Exception("IMEI not found.");

        imei.Status = "Defective";
        imei.IsSynced = false;
        imei.UpdatedAtUtc = DateTime.UtcNow;

        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var imei = db.ImeiUnits.Find(id);
        if (imei == null) return;

        imei.IsDeleted = true;
        imei.IsSynced = false;
        imei.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    public int GetAvailableCount(Guid productGuid)
    {
        using var db = new LocalDbContext();
        return db.ImeiUnits
            .Count(i => i.ProductGuid == productGuid && i.Status == "Available" && !i.IsDeleted);
    }
}
