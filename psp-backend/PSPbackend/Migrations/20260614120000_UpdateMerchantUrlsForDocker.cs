using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSPbackend.Migrations
{
    public partial class UpdateMerchantUrlsForDocker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Merchants
                SET
                    SucessUrl = REPLACE(SucessUrl, 'http://localhost:5173', 'http://shop1.localhost:8080'),
                    FailedUrl  = REPLACE(FailedUrl,  'http://localhost:5173', 'http://shop1.localhost:8080'),
                    ErrorUrl   = REPLACE(ErrorUrl,   'http://localhost:5173', 'http://shop1.localhost:8080')
                WHERE
                    SucessUrl LIKE '%localhost:5173%'
                    OR FailedUrl  LIKE '%localhost:5173%'
                    OR ErrorUrl   LIKE '%localhost:5173%'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Merchants
                SET
                    SucessUrl = REPLACE(SucessUrl, 'http://shop1.localhost:8080', 'http://localhost:5173'),
                    FailedUrl  = REPLACE(FailedUrl,  'http://shop1.localhost:8080', 'http://localhost:5173'),
                    ErrorUrl   = REPLACE(ErrorUrl,   'http://shop1.localhost:8080', 'http://localhost:5173')
                WHERE
                    SucessUrl LIKE '%shop1.localhost:8080%'
                    OR FailedUrl  LIKE '%shop1.localhost:8080%'
                    OR ErrorUrl   LIKE '%shop1.localhost:8080%'
            ");
        }
    }
}
