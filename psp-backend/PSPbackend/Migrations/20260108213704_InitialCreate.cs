using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSPbackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    MerchantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MerchantPassword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.MerchantId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MerchantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MerchantOrderId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MerchantTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    Stan = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PspTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BankMerchantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankPaymentRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcquirerTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.TransactionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_MerchantId",
                table: "Merchants",
                column: "MerchantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_BankPaymentRequestId",
                table: "PaymentTransactions",
                column: "BankPaymentRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_MerchantId_Stan_PspTimestamp",
                table: "PaymentTransactions",
                columns: new[] { "MerchantId", "Stan", "PspTimestamp" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");
        }
    }
}
