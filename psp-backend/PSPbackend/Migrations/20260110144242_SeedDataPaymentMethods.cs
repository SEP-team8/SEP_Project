using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSPbackend.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================
            // PaymentMethods
            // =========================

            var cardId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var qrId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var paypalId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var cryptoId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "PaymentMethodId", "PaymentMethodType" },
                values: new object[,]
                {
                    { cardId,   0 }, // Card
                    { qrId,     1 }, // QrCode
                    { paypalId, 2 }, // PayPal
                    { cryptoId, 3 }  // Crypto
                });

            // =========================
            // MerchantPaymentMethods
            // =========================

            var merchantId = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");

            migrationBuilder.InsertData(
                table: "MerchantPaymentMethods",
                columns: new[] { "MerchantId", "PaymentMethodId" },
                values: new object[,]
                {
                    { merchantId, cardId },
                    { merchantId, qrId }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
