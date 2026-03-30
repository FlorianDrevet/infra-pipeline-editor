using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContainerRegistryId",
                table: "WebApps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeploymentMode",
                table: "WebApps",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DockerImageName",
                table: "WebApps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DockerImageTag",
                table: "WebAppEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContainerRegistryId",
                table: "FunctionApps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeploymentMode",
                table: "FunctionApps",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DockerImageName",
                table: "FunctionApps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DockerImageTag",
                table: "FunctionAppEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContainerRegistryId",
                table: "ContainerApps",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContainerRegistryId",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "DeploymentMode",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "DockerImageName",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "DockerImageTag",
                table: "WebAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "ContainerRegistryId",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "DeploymentMode",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "DockerImageName",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "DockerImageTag",
                table: "FunctionAppEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "ContainerRegistryId",
                table: "ContainerApps");
        }
    }
}
