using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webshop_back.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Users");
        }
    }
}
