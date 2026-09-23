namespace POSApp.Data.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid CustomerGuid { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = "Paid"; // Paid / Unpaid / Partial / Returned / Partially Returned
        public string SaleType { get; set; } = "Cash"; // Cash / Credit
        public int BranchId { get; set; } = 1;
        public decimal TaxRatePercent { get; set; }
        public string PaymentStatus { get; set; } = "Paid";

        public string CreatedByDeviceId { get; set; } = string.Empty;

        public List<InvoiceItem> Items { get; set; } = new();

        // Sync fields
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsSynced { get; set; } = false;
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid InvoiceGuid { get; set; }
        public Guid ProductGuid { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? IMEI { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

        // Sync fields
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsSynced { get; set; } = false;
    }
}
