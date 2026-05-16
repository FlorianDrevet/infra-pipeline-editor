using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDockerImageValidatedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DockerImageValidated",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DockerImageValidated",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DockerImageValidated",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DockerImageValidated",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "DockerImageValidated",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "DockerImageValidated",
                table: "FunctionApps");
        }
    }
}
