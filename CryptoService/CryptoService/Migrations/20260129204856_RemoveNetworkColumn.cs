using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNetworkColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Network",
                table: "CryptoPayments");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "Network",
                table: "CryptoPayments",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }
    }
}
