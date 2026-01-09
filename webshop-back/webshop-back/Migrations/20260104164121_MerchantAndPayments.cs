using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webshop_back.Migrations
{
    /// <inheritdoc />
    public partial class MerchantAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants");

            migrationBuilder.AlterColumn<string>(
                name: "Stan",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentUrl",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiKeyHash",
                table: "Merchants",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "AllowedReturnUrls",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Merchants",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApiKeyRotatedAt",
                table: "Merchants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Merchants",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Merchants",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Merchants",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_MerchantId",
                table: "Merchants",
                column: "MerchantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_MerchantId",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "ApiKeyRotatedAt",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Merchants");

            migrationBuilder.AlterColumn<string>(
                name: "Stan",
                table: "Orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentUrl",
                table: "Orders",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiKeyHash",
                table: "Merchants",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "AllowedReturnUrls",
                table: "Merchants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants",
                column: "MerchantId");
        }
    }
}
