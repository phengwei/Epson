using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epson.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSLASettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AactionDetails",
                table: "AuditTrail");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "AuditTrail");

            migrationBuilder.DropColumn(
                name: "UpdatedOnUTC",
                table: "AuditTrail");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "RequestProduct",
                newName: "FulfilledPrice");

            migrationBuilder.RenameColumn(
                name: "productId",
                table: "ProductCategory",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "categoryId",
                table: "ProductCategory",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "AuditTrail",
                newName: "ActionDetails");

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "RequestProduct",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Breached",
                table: "Request",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SLASetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IncludeHoliday = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeStaffLeaves = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeWorkingHours = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WorkingStartHour = table.Column<int>(type: "int", nullable: false),
                    WorkingStartMinute = table.Column<int>(type: "int", nullable: false),
                    WorkingEndHour = table.Column<int>(type: "int", nullable: false),
                    WorkingEndMinute = table.Column<int>(type: "int", nullable: false),
                    DeadlineInHours = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLASetting", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SLASetting");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "Breached",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "FulfilledPrice",
                table: "RequestProduct",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductCategory",
                newName: "productId");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "ProductCategory",
                newName: "categoryId");

            migrationBuilder.RenameColumn(
                name: "ActionDetails",
                table: "AuditTrail",
                newName: "UpdatedById");

            migrationBuilder.AddColumn<string>(
                name: "AactionDetails",
                table: "AuditTrail",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "AuditTrail",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOnUTC",
                table: "AuditTrail",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
