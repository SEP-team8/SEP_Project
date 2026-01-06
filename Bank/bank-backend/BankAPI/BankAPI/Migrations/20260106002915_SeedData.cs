using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================
            // BANK ACCOUNT
            // =====================
            migrationBuilder.InsertData(
                table: "BankAccounts",
                columns: new[] { "AccountId", "AccountNumber", "Balance", "Currency" },
                values: new object[]
                {
                    new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    "123-4567890123456-78",
                    10000.00m,
                    1
                });

            // =====================
            // PSP
            // =====================
            migrationBuilder.InsertData(
                table: "Psps",
                columns: new[] { "Id", "Name", "HMACKey" },
                values: new object[]
                {
                    new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    "Demo PSP",
                    "super_secret_hmac_key_123"
                });

            // =====================
            // MERCHANT
            // =====================
            migrationBuilder.InsertData(
                table: "Merchants",
                columns: new[] { "Id", "Name", "AccountId" },
                values: new object[]
                {
                    new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    "Demo Web Shop",
                    new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                });

            // =====================
            // CARD (not PCI-safe)
            // =====================
            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[]
                {
                    "CardId",
                    "PAN",
                    "CardholderName",
                    "ExpiryMmYy",
                    "AccountId"
                },
                values: new object[]
                {
                    new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    "4111111111111111",
                    "TEST USER",
                    "12/28",
                    new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
