using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSPbackend.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Merchants",
            columns: new[]
            {
                "MerchantId",
                "BankMerchantId",
                "MerchantPassword",
                "FailedUrl",
                "SucessUrl",
                "ErrorUrl"
            },
            values: new object[]
            {
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                "psp-secret-123",
                "https://example.com/failed",
                "https://example.com/success",
                "https://example.com/error"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
