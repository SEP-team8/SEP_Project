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
                    "105-0000000000000-29",
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
            // CARD
            // PAN stored as SHA256("4111111111111111"), CVV as plaintext '123' (encrypted on first use)
            // Using Sql() to avoid EF model-mapping issues with new column names
            // =====================
            migrationBuilder.Sql(@"
                INSERT INTO [Cards] ([CardId], [PanHash], [PanLast4], [EncryptedCvv], [CardholderName], [ExpiryMmYy], [AccountId])
                VALUES (
                    'dddddddd-dddd-dddd-dddd-dddddddddddd',
                    '9bbef19476623ca56c17da75fd57734dbf82530686043a6e491c6d71befe8f6e',
                    '1111',
                    '123',
                    'TEST USER',
                    '12/28',
                    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
