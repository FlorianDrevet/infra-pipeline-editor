using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerAppEnvironmentAndContainerApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerAppEnvironments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerAppEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerAppEnvironments_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerAppEnvironmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerApps_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerAppEnvironmentEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerAppEnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WorkloadProfileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InternalLoadBalancerEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    ZoneRedundancyEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    LogAnalyticsWorkspaceId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerAppEnvironmentEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerAppEnvironmentEnvironmentSettings_ContainerAppEnvi~",
                        column: x => x.ContainerAppEnvironmentId,
                        principalTable: "ContainerAppEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerAppEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContainerImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CpuCores = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MemoryGi = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MinReplicas = table.Column<int>(type: "integer", nullable: true),
                    MaxReplicas = table.Column<int>(type: "integer", nullable: true),
                    IngressEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    IngressTargetPort = table.Column<int>(type: "integer", nullable: true),
                    IngressExternal = table.Column<bool>(type: "boolean", nullable: true),
                    TransportMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerAppEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerAppEnvironmentSettings_ContainerApps_ContainerAppId",
                        column: x => x.ContainerAppId,
                        principalTable: "ContainerApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContainerAppEnvironmentEnvironmentSettings_ContainerAppEnvi~",
                table: "ContainerAppEnvironmentEnvironmentSettings",
                columns: new[] { "ContainerAppEnvironmentId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContainerAppEnvironmentSettings_ContainerAppId_EnvironmentN~",
                table: "ContainerAppEnvironmentSettings",
                columns: new[] { "ContainerAppId", "EnvironmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContainerAppEnvironmentEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ContainerAppEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ContainerAppEnvironments");

            migrationBuilder.DropTable(
                name: "ContainerApps");
        }
    }
}
