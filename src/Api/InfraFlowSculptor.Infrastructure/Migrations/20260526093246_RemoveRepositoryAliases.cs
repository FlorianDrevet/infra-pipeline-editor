using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRepositoryAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_ProjectRepositories_ProjectId_Alias";
                DROP INDEX IF EXISTS "IX_InfraConfigRepositories_InfrastructureConfigId_Alias";
                ALTER TABLE "ProjectRepositories" DROP COLUMN IF EXISTS "Alias";
                ALTER TABLE "InfraConfigRepositories" DROP COLUMN IF EXISTS "Alias";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "ProjectRepositories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "InfraConfigRepositories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepositories_ProjectId_Alias",
                table: "ProjectRepositories",
                columns: new[] { "ProjectId", "Alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InfraConfigRepositories_InfrastructureConfigId_Alias",
                table: "InfraConfigRepositories",
                columns: new[] { "InfrastructureConfigId", "Alias" },
                unique: true);
        }
    }
}
