using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epson.Data.Migrations
{
    /// <inheritdoc />
    public partial class databaseschemachanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Budget",
                table: "RequestProduct",
                newName: "EndUserPrice");

            migrationBuilder.AlterColumn<string>(
                name: "FulfillerId",
                table: "RequestProduct",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Breached",
                table: "RequestProduct",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUTC",
                table: "RequestProduct",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "DealerPrice",
                table: "RequestProduct",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "RequestProduct",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "DistyPrice",
                table: "RequestProduct",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCoverplus",
                table: "RequestProduct",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "RequestProduct",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RequestProduct",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TenderDate",
                table: "RequestProduct",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeToResolution",
                table: "RequestProduct",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOnUTC",
                table: "RequestProduct",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Request",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Breached",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "CreatedOnUTC",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "DealerPrice",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "DistyPrice",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "IsCoverplus",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "TenderDate",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "TimeToResolution",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "UpdatedOnUTC",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "EndUserPrice",
                table: "RequestProduct",
                newName: "Budget");

            migrationBuilder.UpdateData(
                table: "RequestProduct",
                keyColumn: "FulfillerId",
                keyValue: null,
                column: "FulfillerId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "FulfillerId",
                table: "RequestProduct",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
