using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSPbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCryptoAmountField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CryptoAmount",
                table: "PaymentTransactions",
                type: "decimal(36,18)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CryptoAmount",
                table: "PaymentTransactions");
        }
    }
}
