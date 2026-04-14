using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PoolProjectNotConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentPoolName",
                table: "InfrastructureConfigs");

            migrationBuilder.AddColumn<string>(
                name: "AgentPoolName",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentPoolName",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "AgentPoolName",
                table: "InfrastructureConfigs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
