using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoService.Migrations
{
    /// <inheritdoc />
    public partial class AddCryptoFieldsToCryptoPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "WalletAddresses",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "CryptoPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PspTimestamp",
                table: "CryptoPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stan",
                table: "CryptoPayments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "CryptoPayments");

            migrationBuilder.DropColumn(
                name: "PspTimestamp",
                table: "CryptoPayments");

            migrationBuilder.DropColumn(
                name: "Stan",
                table: "CryptoPayments");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "WalletAddresses",
                type: "decimal(38,18)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
