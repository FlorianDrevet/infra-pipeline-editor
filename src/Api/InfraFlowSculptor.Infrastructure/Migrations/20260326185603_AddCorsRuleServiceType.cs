using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCorsRuleServiceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "StorageAccountCorsRules",
                type: "text",
                nullable: false,
                defaultValue: "Blob");

            // Backfill existing rows that got the empty-string default before this fix
            migrationBuilder.Sql(
                """UPDATE "StorageAccountCorsRules" SET "ServiceType" = 'Blob' WHERE "ServiceType" = '';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "StorageAccountCorsRules");
        }
    }
}
