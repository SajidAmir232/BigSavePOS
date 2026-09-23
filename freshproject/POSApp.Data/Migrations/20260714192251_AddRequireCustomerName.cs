using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequireCustomerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireCustomerName",
                table: "CompanySettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Guid", "RequireCustomerName", "UpdatedAtUtc" },
                values: new object[] { new Guid("15da91a1-da76-4bf1-81b0-538c733f9e36"), false, new DateTime(2026, 7, 14, 19, 22, 49, 298, DateTimeKind.Utc).AddTicks(5500) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAtUtc",
                value: new DateTime(2026, 7, 14, 19, 22, 49, 298, DateTimeKind.Utc).AddTicks(4715));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequireCustomerName",
                table: "CompanySettings");

            migrationBuilder.UpdateData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Guid", "UpdatedAtUtc" },
                values: new object[] { new Guid("9a819fe3-b960-42b4-9871-6dc521351077"), new DateTime(2026, 7, 14, 14, 41, 11, 372, DateTimeKind.Utc).AddTicks(6819) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAtUtc",
                value: new DateTime(2026, 7, 14, 14, 41, 11, 372, DateTimeKind.Utc).AddTicks(6134));
        }
    }
}
