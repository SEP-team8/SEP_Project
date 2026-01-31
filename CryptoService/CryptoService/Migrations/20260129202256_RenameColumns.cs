using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoService.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BitcoinAmount",
                table: "CryptoPayments",
                newName: "CryptoCurrencyAmount");

            migrationBuilder.RenameColumn(
                name: "BitcoinAddress",
                table: "CryptoPayments",
                newName: "CryptoCurrencyAddress");

            migrationBuilder.RenameIndex(
                name: "IX_CryptoPayments_BitcoinAddress",
                table: "CryptoPayments",
                newName: "IX_CryptoPayments_CryptoCurrencyAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CryptoCurrencyAmount",
                table: "CryptoPayments",
                newName: "BitcoinAmount");

            migrationBuilder.RenameColumn(
                name: "CryptoCurrencyAddress",
                table: "CryptoPayments",
                newName: "BitcoinAddress");

            migrationBuilder.RenameIndex(
                name: "IX_CryptoPayments_CryptoCurrencyAddress",
                table: "CryptoPayments",
                newName: "IX_CryptoPayments_BitcoinAddress");
        }
    }
}
