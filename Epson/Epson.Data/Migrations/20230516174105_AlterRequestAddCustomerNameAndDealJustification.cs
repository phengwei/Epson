using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epson.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterRequestAddCustomerNameAndDealJustification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Request",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DealJustification",
                table: "Request",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "DealJustification",
                table: "Request");
        }
    }
}
