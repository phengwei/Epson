using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epson.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterRequestTablesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "ManagerName",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Request",
                newName: "ApprovalState");

            migrationBuilder.AddColumn<string>(
                name: "FulfillerId",
                table: "RequestProduct",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "HasFulfilled",
                table: "RequestProduct",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "RequestProduct",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBudget",
                table: "Request",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FulfillerId",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "HasFulfilled",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "RequestProduct");

            migrationBuilder.DropColumn(
                name: "TotalBudget",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "ApprovalState",
                table: "Request",
                newName: "Quantity");

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "Request",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManagerName",
                table: "Request",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
