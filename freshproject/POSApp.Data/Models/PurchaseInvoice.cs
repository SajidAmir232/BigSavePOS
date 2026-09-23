namespace POSApp.Data.Models
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string PurchaseNumber { get; set; } = string.Empty;
    public int BranchId { get; set; } = 1;
        public Guid SupplierGuid { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public decimal SubTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = "Unpaid";

        public List<PurchaseInvoiceItem> Items { get; set; } = new();

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsSynced { get; set; } = false;
    }

    public class PurchaseInvoiceItem
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid PurchaseInvoiceGuid { get; set; }
        public Guid ProductGuid { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Total { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsSynced { get; set; } = false;
    }
}
