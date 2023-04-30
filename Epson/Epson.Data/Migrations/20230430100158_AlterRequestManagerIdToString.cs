using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epson.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterRequestManagerIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Email_Queue",
                table: "Email_Queue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Email_Account",
                table: "Email_Account");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audit_Trail",
                table: "Audit_Trail");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Email_Queue");

            migrationBuilder.DropColumn(
                name: "CreatedOnUTC",
                table: "Email_Queue");

            migrationBuilder.DropColumn(
                name: "From",
                table: "Email_Queue");

            migrationBuilder.DropColumn(
                name: "UpdatedOnUTC",
                table: "Email_Queue");

            migrationBuilder.RenameTable(
                name: "Email_Queue",
                newName: "EmailQueue");

            migrationBuilder.RenameTable(
                name: "Email_Account",
                newName: "EmailAccount");

            migrationBuilder.RenameTable(
                name: "Audit_Trail",
                newName: "AuditTrail");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "EmailQueue",
                newName: "ToEmail");

            migrationBuilder.RenameColumn(
                name: "To",
                table: "EmailQueue",
                newName: "FromEmail");

            migrationBuilder.AlterColumn<string>(
                name: "ManagerId",
                table: "Request",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentTime",
                table: "EmailQueue",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailQueue",
                table: "EmailQueue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailAccount",
                table: "EmailAccount",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditTrail",
                table: "AuditTrail",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailQueue",
                table: "EmailQueue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailAccount",
                table: "EmailAccount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditTrail",
                table: "AuditTrail");

            migrationBuilder.RenameTable(
                name: "EmailQueue",
                newName: "Email_Queue");

            migrationBuilder.RenameTable(
                name: "EmailAccount",
                newName: "Email_Account");

            migrationBuilder.RenameTable(
                name: "AuditTrail",
                newName: "Audit_Trail");

            migrationBuilder.RenameColumn(
                name: "ToEmail",
                table: "Email_Queue",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "FromEmail",
                table: "Email_Queue",
                newName: "To");

            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Request",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentTime",
                table: "Email_Queue",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Email_Queue",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUTC",
                table: "Email_Queue",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "From",
                table: "Email_Queue",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOnUTC",
                table: "Email_Queue",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Email_Queue",
                table: "Email_Queue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Email_Account",
                table: "Email_Account",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audit_Trail",
                table: "Audit_Trail",
                column: "Id");
        }
    }
}
