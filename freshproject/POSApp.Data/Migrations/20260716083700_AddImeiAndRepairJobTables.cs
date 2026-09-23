using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImeiAndRepairJobTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImeiUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    SalePrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SoldDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InvoiceGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSynced = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImeiUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceModel = table.Column<string>(type: "TEXT", nullable: false),
                    IMEI = table.Column<string>(type: "TEXT", nullable: true),
                    ProblemDescription = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ActualCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    TechnicianName = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateDelivered = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSynced = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairJobParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    RepairJobGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSynced = table.Column<bool>(type: "INTEGER", nullable: false),
                    RepairJobId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairJobParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairJobParts_RepairJobs_RepairJobId",
                        column: x => x.RepairJobId,
                        principalTable: "RepairJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Guid", "UpdatedAtUtc" },
                values: new object[] { new Guid("44bc01e8-ada7-4d58-8aa6-5d388bc2e866"), new DateTime(2026, 7, 16, 8, 36, 58, 801, DateTimeKind.Utc).AddTicks(743) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAtUtc",
                value: new DateTime(2026, 7, 16, 8, 36, 58, 801, DateTimeKind.Utc).AddTicks(443));

            migrationBuilder.CreateIndex(
                name: "IX_ImeiUnits_Guid",
                table: "ImeiUnits",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImeiUnits_SerialNumber",
                table: "ImeiUnits",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairJobParts_Guid",
                table: "RepairJobParts",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairJobParts_RepairJobId",
                table: "RepairJobParts",
                column: "RepairJobId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairJobs_Guid",
                table: "RepairJobs",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImeiUnits");

            migrationBuilder.DropTable(
                name: "RepairJobParts");

            migrationBuilder.DropTable(
                name: "RepairJobs");

            migrationBuilder.UpdateData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Guid", "UpdatedAtUtc" },
                values: new object[] { new Guid("15da91a1-da76-4bf1-81b0-538c733f9e36"), new DateTime(2026, 7, 14, 19, 22, 49, 298, DateTimeKind.Utc).AddTicks(5500) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAtUtc",
                value: new DateTime(2026, 7, 14, 19, 22, 49, 298, DateTimeKind.Utc).AddTicks(4715));
        }
    }
}
