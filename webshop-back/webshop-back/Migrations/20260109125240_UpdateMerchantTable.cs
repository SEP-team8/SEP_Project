using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webshop_back.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMerchantTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PspConfigSnapshot",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PspEnvironment",
                table: "Merchants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PspMerchantId",
                table: "Merchants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PspMerchantSecret",
                table: "Merchants",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PspConfigSnapshot",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "PspEnvironment",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "PspMerchantId",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "PspMerchantSecret",
                table: "Merchants");
        }
    }
}
