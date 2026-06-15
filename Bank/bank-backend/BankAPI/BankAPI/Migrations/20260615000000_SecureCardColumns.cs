using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankAPI.Migrations
{
    /// <inheritdoc />
    [Migration("20260615000000_SecureCardColumns")]
    public partial class SecureCardColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the unique index on the plaintext PAN column before altering it
            migrationBuilder.DropIndex(
                name: "IX_Cards_PAN",
                table: "Cards");

            // Add new columns as nullable first so we can populate them before enforcing NOT NULL
            migrationBuilder.AddColumn<string>(
                name: "PanHash",
                table: "Cards",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanLast4",
                table: "Cards",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedCvv",
                table: "Cards",
                type: "nvarchar(max)",
                nullable: true);

            // Migrate existing data:
            // - PanHash: SHA256 of PAN as VARCHAR bytes (matches C# SHA256.HashData(Encoding.UTF8.GetBytes(pan)) for ASCII PAN values)
            // - PanLast4: last 4 digits of PAN
            // - EncryptedCvv: store as plaintext for now; PaymentService will re-protect on first use (wasPlaintext fallback)
            //   Seed card had Cvv = '' (default value), so we set a usable test value of '123'
            migrationBuilder.Sql(@"
                UPDATE [Cards] SET
                    [PanHash]      = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(19), [PAN])), 2)),
                    [PanLast4]     = RIGHT([PAN], 4),
                    [EncryptedCvv] = CASE WHEN [Cvv] IS NULL OR LEN([Cvv]) <> 3 THEN '123' ELSE [Cvv] END
            ");

            // Enforce NOT NULL now that data is populated
            migrationBuilder.AlterColumn<string>(
                name: "PanHash",
                table: "Cards",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PanLast4",
                table: "Cards",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedCvv",
                table: "Cards",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Drop the old plaintext columns
            migrationBuilder.DropColumn(
                name: "PAN",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "Cvv",
                table: "Cards");

            // Create unique index on the new hash column
            migrationBuilder.CreateIndex(
                name: "IX_Cards_PanHash",
                table: "Cards",
                column: "PanHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_PanHash",
                table: "Cards");

            // Restore plaintext columns (data cannot be recovered from hash — columns will be empty)
            migrationBuilder.AddColumn<string>(
                name: "PAN",
                table: "Cards",
                type: "nvarchar(19)",
                maxLength: 19,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cvv",
                table: "Cards",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "PanHash",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "PanLast4",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "EncryptedCvv",
                table: "Cards");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_PAN",
                table: "Cards",
                column: "PAN",
                unique: true);
        }
    }
}
