using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PipelineApplicatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                table: "WebApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppPipelineMode",
                table: "InfrastructureConfigs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Isolated");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                table: "FunctionApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                table: "ContainerApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationName",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "AppPipelineMode",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "ApplicationName",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "ApplicationName",
                table: "ContainerApps");
        }
    }
}
