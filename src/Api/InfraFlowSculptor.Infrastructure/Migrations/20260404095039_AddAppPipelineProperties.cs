using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppPipelineProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuildCommand",
                table: "WebApps",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DockerfilePath",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCodePath",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildCommand",
                table: "FunctionApps",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DockerfilePath",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCodePath",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DockerfilePath",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "DockerfilePath",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "SourceCodePath",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "BuildCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "DockerfilePath",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "SourceCodePath",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "DockerfilePath",
                table: "ContainerApps");
        }
    }
}
