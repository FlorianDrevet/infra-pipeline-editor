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
            migrationBuilder.DropIndex(
                name: "IX_ProjectRepositories_ProjectId_Alias",
                table: "ProjectRepositories");

            migrationBuilder.DropIndex(
                name: "IX_InfraConfigRepositories_InfrastructureConfigId_Alias",
                table: "InfraConfigRepositories");

            migrationBuilder.DropColumn(
                name: "Alias",
                table: "ProjectRepositories");

            migrationBuilder.DropColumn(
                name: "Alias",
                table: "InfraConfigRepositories");
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