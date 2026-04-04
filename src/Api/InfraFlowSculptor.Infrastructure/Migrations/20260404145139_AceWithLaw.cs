using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AceWithLaw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogAnalyticsWorkspaceId",
                table: "ContainerAppEnvironmentEnvironmentSettings");

            migrationBuilder.AddColumn<Guid>(
                name: "LogAnalyticsWorkspaceId",
                table: "ContainerAppEnvironments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogAnalyticsWorkspaceId",
                table: "ContainerAppEnvironments");

            migrationBuilder.AddColumn<string>(
                name: "LogAnalyticsWorkspaceId",
                table: "ContainerAppEnvironmentEnvironmentSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
