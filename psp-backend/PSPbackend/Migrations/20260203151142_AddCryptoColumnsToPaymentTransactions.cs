using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSPbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCryptoColumnsToPaymentTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CryptoAddress",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CryptoPaymentId",
                table: "PaymentTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionHash",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CryptoAddress",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "CryptoPaymentId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionHash",
                table: "PaymentTransactions");
        }
    }
}
