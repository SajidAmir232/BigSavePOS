using POSApp.Data.Models;

namespace POSApp.Web;

public static class InvoicePrintHelper
{
    public static string BuildInvoiceHtml(Invoice invoice, List<InvoiceItem> items, Customer? customer, string companyName, string address, string taxNumber)
    {
        var rows = "";
        int idx = 1;
        foreach (var item in items)
            rows += "<tr><td>" + idx++ + "</td><td>" + item.ProductName + "</td><td>" + item.Quantity + "</td><td>Rs" + item.UnitPrice.ToString("N0") + "</td><td>Rs" + item.Total.ToString("N0") + "</td></tr>";

        var statusClass = invoice.Status == "Paid" ? "badge-green" : "badge-red";
        var balanceColor = (invoice.GrandTotal - invoice.PaidAmount > 0) ? "#ef4444" : "#10b981";
        var taxHtml = !string.IsNullOrEmpty(taxNumber) ? "<div style=\"font-size:11px;color:#64748b\">Tax#: " + taxNumber + "</div>" : "";
        var custPhone = customer != null ? "<div style=\"color:#64748b;font-size:12px\">" + customer.Phone + "</div>" : "";

        return "<!DOCTYPE html><html><head><title>Invoice " + invoice.InvoiceNumber + "</title>" +
            "<style>" +
            "body{font-family:Arial,sans-serif;margin:20px;color:#1e293b;font-size:13px}" +
            ".hdr{display:flex;justify-content:space-between;border-bottom:3px solid #4f46e5;padding-bottom:12px;margin-bottom:16px}" +
            ".ttl{font-size:24px;font-weight:700;color:#4f46e5}" +
            "table{width:100%;border-collapse:collapse}" +
            "th{background:#4f46e5;color:#fff;padding:8px;text-align:left;font-size:11px}" +
            "td{padding:8px;border-bottom:1px solid #e2e8f0}" +
            ".tot{font-weight:700;font-size:14px}" +
            ".ft{margin-top:20px;text-align:center;color:#64748b;font-size:11px;border-top:1px solid #e2e8f0;padding-top:8px}" +
            ".badge{display:inline-block;padding:3px 8px;border-radius:4px;font-size:11px;font-weight:600}" +
            ".badge-green{background:#d1fae5;color:#065f46}" +
            ".badge-red{background:#fee2e2;color:#991b1b}" +
            "@media print{body{padding:0}.nb{display:none}}" +
            "</style></head><body>" +
            "<div class=\"hdr\"><div><div class=\"ttl\">" + companyName + "</div><div style=\"color:#64748b\">" + address + "</div>" + taxHtml + "</div>" +
            "<div style=\"text-align:right\"><div style=\"font-size:18px;font-weight:700;color:#4f46e5\">INVOICE</div>" +
            "<div><strong>#" + invoice.InvoiceNumber + "</strong></div>" +
            "<div>" + invoice.InvoiceDate.ToString("dd/MM/yyyy hh:mm tt") + "</div>" +
            "<div style=\"margin-top:4px\"><span class=\"badge " + statusClass + "\">" + invoice.Status + "</span></div></div></div>" +
            "<div style=\"display:flex;gap:12px;margin-bottom:16px\">" +
            "<div style=\"background:#f8fafc;padding:12px;border-radius:8px;flex:1\"><label style=\"font-size:10px;color:#64748b;font-weight:600\">BILL TO</label><div style=\"font-weight:600\">" + (customer?.Name ?? "Walk-in Customer") + "</div>" + custPhone + "</div>" +
            "<div style=\"background:#f8fafc;padding:12px;border-radius:8px;flex:1\"><label style=\"font-size:10px;color:#64748b;font-weight:600\">PAYMENT</label>" +
            "<div>Type: <strong>" + invoice.SaleType + "</strong></div>" +
            "<div>Paid: <strong style=\"color:#10b981\">Rs" + invoice.PaidAmount.ToString("N0") + "</strong></div>" +
            "<div>Balance: <strong style=\"color:" + balanceColor + "\">Rs" + (invoice.GrandTotal - invoice.PaidAmount).ToString("N0") + "</strong></div></div></div>" +
            "<table><thead><tr><th>#</th><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr></thead><tbody>" + rows +
            "<tr class=\"tot\"><td colspan=\"4\" style=\"text-align:right\">Subtotal</td><td>Rs" + invoice.SubTotal.ToString("N0") + "</td></tr>" +
            "<tr class=\"tot\"><td colspan=\"4\" style=\"text-align:right\">Discount</td><td>-Rs" + invoice.Discount.ToString("N0") + "</td></tr>" +
            "<tr class=\"tot\"><td colspan=\"4\" style=\"text-align:right\">Tax</td><td>Rs" + invoice.Tax.ToString("N0") + "</td></tr>" +
            "<tr class=\"tot\" style=\"color:#4f46e5;font-size:16px\"><td colspan=\"4\" style=\"text-align:right\">Grand Total</td><td>Rs" + invoice.GrandTotal.ToString("N0") + "</td></tr>" +
            "</tbody></table><div class=\"ft\">Thank you for your purchase! | " + companyName + "</div>" +
            "<div class=\"nb\" style=\"text-align:center;margin-top:16px\"><button onclick=\"window.print()\" style=\"background:#4f46e5;color:#fff;border:none;padding:10px 24px;border-radius:6px;font-size:14px;cursor:pointer;font-weight:600\">Print Invoice</button></div>" +
            "</body></html>";
    }
}
