using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthProbeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LivenessProbePath",
                table: "ContainerAppEnvironmentSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LivenessProbePort",
                table: "ContainerAppEnvironmentSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReadinessProbePath",
                table: "ContainerAppEnvironmentSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReadinessProbePort",
                table: "ContainerAppEnvironmentSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartupProbePath",
                table: "ContainerAppEnvironmentSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartupProbePort",
                table: "ContainerAppEnvironmentSettings",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LivenessProbePath",
                table: "ContainerAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "LivenessProbePort",
                table: "ContainerAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "ReadinessProbePath",
                table: "ContainerAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "ReadinessProbePort",
                table: "ContainerAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "StartupProbePath",
                table: "ContainerAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "StartupProbePort",
                table: "ContainerAppEnvironmentSettings");
        }
    }
}
