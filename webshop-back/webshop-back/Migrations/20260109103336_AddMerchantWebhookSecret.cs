using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webshop_back.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantWebhookSecret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebhookSecret",
                table: "Merchants",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookSecret",
                table: "Merchants");
        }
    }
}
