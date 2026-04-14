using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigAzureFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RuntimeStack",
                table: "WebAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "RuntimeVersion",
                table: "WebAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "RuntimeStack",
                table: "FunctionAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "RuntimeVersion",
                table: "FunctionAppEnvironmentSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RuntimeStack",
                table: "WebAppEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuntimeVersion",
                table: "WebAppEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuntimeStack",
                table: "FunctionAppEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuntimeVersion",
                table: "FunctionAppEnvironmentSettings",
                type: "text",
                nullable: true);
        }
    }
}
