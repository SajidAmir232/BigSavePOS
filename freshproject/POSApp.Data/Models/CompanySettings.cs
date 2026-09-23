namespace POSApp.Data.Models;

public class CompanySettings
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = "My POS Shop";
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? LogoPath { get; set; }
    public decimal TaxRatePercent { get; set; } = 0m;
    public bool RequireCustomerName { get; set; } = false;
    public bool ShowCustomerField { get; set; } = true;
    public bool RequireExpiryDate { get; set; } = false;
    public string? ApiServerUrl { get; set; }
    public DateTime? LastBackupDate { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
