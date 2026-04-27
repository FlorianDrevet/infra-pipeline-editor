using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcrAuthModeToComputeApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcrAuthMode",
                table: "WebApps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcrAuthMode",
                table: "FunctionApps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcrAuthMode",
                table: "ContainerApps",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcrAuthMode",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "AcrAuthMode",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "AcrAuthMode",
                table: "ContainerApps");
        }
    }
}
