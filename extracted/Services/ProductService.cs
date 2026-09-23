using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class ProductService
{
    public List<Product> GetAll() { using var db = new LocalDbContext(); return db.Products.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList(); }
    public List<Product> GetVisible(User user)
    {
        using var db = new LocalDbContext();
        var q = db.Products.Where(p => !p.IsDeleted && p.IsActive);
        if (!PosAuthorizationService.IsAdmin(user)) q = q.Where(p => p.BranchId == (user.PrimaryBranchId ?? 1));
        return q.OrderBy(p => p.Name).ToList();
    }
    public Product? GetByGuid(Guid guid) { using var db = new LocalDbContext(); return db.Products.FirstOrDefault(p => p.Guid == guid && !p.IsDeleted); }
    public Product Save(Product product, User? actor = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.CanManageProducts(actor));
        using var db = new LocalDbContext();
        product.UpdatedAtUtc = DateTime.UtcNow; product.IsSynced = false; product.Sku = string.IsNullOrWhiteSpace(product.Sku) ? GenerateSku(db) : product.Sku.Trim(); product.Barcode = string.IsNullOrWhiteSpace(product.Barcode) ? GenerateBarcode(db, product.BranchId) : product.Barcode.Trim();
        if (product.BranchId <= 0) product.BranchId = actor?.PrimaryBranchId ?? 1;
        if (!string.IsNullOrWhiteSpace(product.Barcode) && db.Products.Any(p => p.Guid != product.Guid && p.BranchId == product.BranchId && p.Barcode == product.Barcode && !p.IsDeleted)) throw new InvalidOperationException("Barcode already exists in this branch.");
        if (product.Id == 0) db.Products.Add(product); else db.Products.Update(product);
        db.SaveChanges(); return product;
    }
    public void Delete(int id, User? actor = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.CanDeleteProducts(actor), "Only an Admin can delete products.");
        using var db = new LocalDbContext(); var product = db.Products.Find(id); if (product == null) return;
        product.IsDeleted = true; product.IsSynced = false; product.UpdatedAtUtc = DateTime.UtcNow; db.SaveChanges();
    }
    public void AdjustStock(Guid productGuid, decimal quantityChange, User? actor = null, string type = "Adjustment", string? reason = null, string? reference = null)
    {
        if (actor != null) PosAuthorizationService.Require(PosAuthorizationService.CanAdjustInventory(actor), "Inventory adjustments require Manager/Admin permission.");
        using var db = new LocalDbContext(); using var tx = db.Database.BeginTransaction();
        var product = db.Products.FirstOrDefault(p => p.Guid == productGuid && !p.IsDeleted); if (product == null) throw new InvalidOperationException("Product not found.");
        var newQty = product.Quantity + (int)quantityChange; if (newQty < 0) throw new InvalidOperationException($"Insufficient stock for {product.Name}.");
        product.Quantity = newQty; product.IsSynced = false; product.UpdatedAtUtc = DateTime.UtcNow;
        db.InventoryTransactions.Add(new InventoryTransaction { ProductGuid = product.Guid, BranchId = product.BranchId, QuantityChange = quantityChange, QuantityAfter = newQty, Type = type, Reason = reason, Reference = reference, CreatedBy = actor?.Username });
        db.SaveChanges(); tx.Commit();
    }
    private string GenerateSku(LocalDbContext db) { var prefix = "PROD-" + DateTime.Now.ToString("yyyyMMdd"); var n = db.Products.Count(p => p.Sku != null && p.Sku.StartsWith(prefix)) + 1; return $"{prefix}-{n:D4}"; }
    private string GenerateBarcode(LocalDbContext db, int branchId)
    {
        var next = db.Products.Where(p => p.BranchId == branchId && p.Barcode != null).Select(p => p.Barcode!).AsEnumerable().Select(value => long.TryParse(value, out var number) ? number % 1000000000000 : 0).DefaultIfEmpty(890000000000).Max() + 1;
        var digits = next.ToString("D12");
        var checksum = digits.Select((digit, index) => (digit - '0') * (index % 2 == 0 ? 1 : 3)).Sum();
        return digits + ((10 - checksum % 10) % 10);
    }
}
