using ClosedXML.Excel;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public sealed class ExcelPosService
{
    public byte[] ProductTemplate()
    {
        using var wb=new XLWorkbook(); var ws=wb.Worksheets.Add("Products");
        var headers=new[]{"Barcode","Product Name","Category","Subcategory","Unit","Purchase Price","Sale Price","Wholesale Price","Opening Stock","Minimum Stock","Expiry Date","Brand","SKU"};
        for(int i=0;i<headers.Length;i++) ws.Cell(1,i+1).Value=headers[i]; ws.Row(1).Style.Font.Bold=true; ws.Columns().AdjustToContents();
        using var ms=new MemoryStream(); wb.SaveAs(ms); return ms.ToArray();
    }
    public byte[] ExportProducts(User actor)
    {
        PosAuthorizationService.Require(PosAuthorizationService.CanExport(actor)); using var db=new LocalDbContext(); var q=db.Products.Where(p=>!p.IsDeleted); if(!PosAuthorizationService.IsAdmin(actor))q=q.Where(p=>p.BranchId==(actor.PrimaryBranchId??1));
        using var wb=new XLWorkbook();var ws=wb.Worksheets.Add("Products");var h=new[]{"Barcode","SKU","Product Name","Category","Subcategory","Unit","Purchase Price","Sale Price","Wholesale Price","Stock","Minimum Stock","Expiry Date","Brand","Branch"};for(int i=0;i<h.Length;i++)ws.Cell(1,i+1).Value=h[i];ws.Row(1).Style.Font.Bold=true;int r=2;foreach(var p in q.OrderBy(x=>x.Name)){var v=new object?[]{p.Barcode,p.Sku,p.Name,p.Category,p.Subcategory,p.Unit,p.PurchasePrice,p.SalePrice,p.WholesalePrice,p.Quantity,p.MinimumStock,p.ExpiryDate?.ToString("yyyy-MM-dd"),p.Brand,p.BranchId};for(int i=0;i<v.Length;i++)ws.Cell(r,i+1).Value=XLCellValue.FromObject(v[i]??"");r++;}ws.Columns().AdjustToContents();using var ms=new MemoryStream();wb.SaveAs(ms);return ms.ToArray();
    }
    public (int imported, int updated, List<string> errors) ImportProducts(Stream stream, User actor, bool preview=false)
    {
        PosAuthorizationService.Require(PosAuthorizationService.CanManageProducts(actor)); using var wb=new XLWorkbook(stream);var ws=wb.Worksheet(1);var errors=new List<string>();int imported=0,updated=0;using var db=new LocalDbContext();int branch=actor.PrimaryBranchId??1;
        foreach(var row in ws.RowsUsed().Skip(1)){try{var barcode=row.Cell(1).GetString().Trim();var name=row.Cell(2).GetString().Trim();if(string.IsNullOrWhiteSpace(name)){errors.Add($"Row {row.RowNumber()}: Product Name required.");continue;}var existing=!string.IsNullOrWhiteSpace(barcode)?db.Products.FirstOrDefault(p=>p.BranchId==branch&&p.Barcode==barcode&&!p.IsDeleted):null;var p=existing??new Product{BranchId=branch,Name=name};p.Name=name;p.Barcode=barcode;p.Category=row.Cell(3).GetString();p.Subcategory=row.Cell(4).GetString();p.Unit=string.IsNullOrWhiteSpace(row.Cell(5).GetString())?"pcs":row.Cell(5).GetString();p.PurchasePrice=row.Cell(6).GetValue<decimal>();p.SalePrice=row.Cell(7).GetValue<decimal>();p.WholesalePrice=row.Cell(8).GetValue<decimal>();p.Quantity=Math.Max(0,row.Cell(9).GetValue<int>());p.MinimumStock=Math.Max(0,row.Cell(10).GetValue<int>());p.ExpiryDate=DateTime.TryParse(row.Cell(11).GetString(),out var d)?d:null;p.Brand=row.Cell(12).GetString();p.Sku=string.IsNullOrWhiteSpace(row.Cell(13).GetString())?p.Sku:row.Cell(13).GetString();p.UpdatedAtUtc=DateTime.UtcNow;p.IsSynced=false;if(!preview){if(existing==null)db.Products.Add(p);else db.Products.Update(p);if(existing==null)imported++;else updated++;}}catch(Exception ex){errors.Add($"Row {row.RowNumber()}: {ex.Message}");}}
        if(!preview)db.SaveChanges();return(imported,updated,errors);
    }
}
