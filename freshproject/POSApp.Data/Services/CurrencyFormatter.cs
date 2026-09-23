namespace POSApp.Data.Services;

public class CurrencyFormatter
{
    private string _currencySymbol = "Rs";
    private string _currencyCode = "PKR";

    public CurrencyFormatter(string currencyCode = "PKR", string currencySymbol = "Rs")
    {
        _currencyCode = currencyCode;
        _currencySymbol = currencySymbol;
    }

    public void SetCurrency(string currencyCode, string currencySymbol)
    {
        _currencyCode = currencyCode;
        _currencySymbol = currencySymbol;
    }

    public string Format(decimal value)
    {
        return $"{_currencySymbol}{value:N2}";
    }

    public string FormatWithCode(decimal value)
    {
        return $"{_currencyCode} {value:N2}";
    }

    public string GetSymbol() => _currencySymbol;
    public string GetCode() => _currencyCode;

    // Get all supported currencies
    public static Dictionary<string, string> GetSupportedCurrencies()
    {
        return new Dictionary<string, string>
        {
            { "PKR", "Rs" },      // Pakistani Rupee
            { "USD", "$" },      // US Dollar
            { "EUR", "€" },      // Euro
            { "GBP", "£" },      // British Pound
            { "AED", "د.إ" },    // UAE Dirham
            { "SAR", "﷼" },      // Saudi Riyal
            { "JPY", "¥" },      // Japanese Yen
            { "CNY", "¥" },      // Chinese Yuan
            { "AUD", "$" },      // Australian Dollar
            { "CAD", "$" },      // Canadian Dollar
        };
    }
}
