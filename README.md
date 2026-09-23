# POSApp v1.0 - .NET 9 General Store POS

This version continues the existing Blazor/.NET POS codebase rather than rewriting it in Next.js.

## Included v1 scope
- POS billing with keyboard/USB barcode scanner workflow
- Barcode, SKU and product-name search
- Stock validation before sale
- Item and invoice discounts
- Configurable tax rate
- Cash, Card, Bank Transfer, Split and Credit payment modes
- Hold/Resume sales
- Invoice printing
- Products with SKU, barcode, category/subcategory, brand, units, cost, retail, wholesale, minimum stock and expiry
- Branch model and branch/user assignment
- Admin / Manager / Cashier server-side authorization helpers
- Branch-scoped product, sales and purchase operations
- Inventory transaction ledger for sales, purchases and adjustments
- Excel product template, validation/preview, import and export
- Database backup service and admin backup UI
- Audit-log infrastructure
- .NET 9 target and .NET 9 Docker deployment
- Existing customers, suppliers, returns, reports, accounting, IMEI and repair modules preserved

## Important verification
The source was modified and packaged here, but this environment does not contain the .NET SDK, so a real `dotnet restore`, `dotnet build`, database migration/runtime test, and browser E2E test could not be executed here. Run those checks before production deployment.

## Run locally
```bash
dotnet restore POSApp.Web/POSApp.Web.csproj
dotnet build POSApp.Web/POSApp.Web.csproj
dotnet run --project POSApp.Web/POSApp.Web.csproj
```

## Docker
```bash
docker compose build
docker compose up -d
```

The SQLite database is stored at `POSAPP_DB_PATH` when configured.


## Sign-in

- Demo administrator: `admin` / `admin123`.
- Sign-in accepts either username or email.
- Existing databases are checked at startup; the bundled legacy demo admin hash is repaired automatically if present.
- User-created accounts authenticate using the password selected during sign-up.
- Passwords are stored as BCrypt hashes; plain-text passwords are not stored.
