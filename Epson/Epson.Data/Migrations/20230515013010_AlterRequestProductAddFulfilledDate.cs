using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epson.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterRequestProductAddFulfilledDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SLASetting");

            migrationBuilder.AddColumn<DateTime>(
                name: "FulfilledDate",
                table: "RequestProduct",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FulfilledDate",
                table: "RequestProduct");

            migrationBuilder.CreateTable(
                name: "SLASetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeadlineInHours = table.Column<int>(type: "int", nullable: false),
                    IncludeHoliday = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeStaffLeaves = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeWorkingHours = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WorkingEndHour = table.Column<int>(type: "int", nullable: false),
                    WorkingEndMinute = table.Column<int>(type: "int", nullable: false),
                    WorkingStartHour = table.Column<int>(type: "int", nullable: false),
                    WorkingStartMinute = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLASetting", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
