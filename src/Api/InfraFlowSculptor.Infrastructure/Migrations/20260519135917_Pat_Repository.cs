using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Pat_Repository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepositories_ProjectId",
                table: "ProjectRepositories",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_InfraConfigRepositories_InfrastructureConfigId",
                table: "InfraConfigRepositories",
                column: "InfrastructureConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectRepositories_ProjectId",
                table: "ProjectRepositories");

            migrationBuilder.DropIndex(
                name: "IX_InfraConfigRepositories_InfrastructureConfigId",
                table: "InfraConfigRepositories");
        }
    }
}
