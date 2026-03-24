using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogAnalyticsWorkspaceAndApplicationInsights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogAnalyticsWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationInsights_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogAnalyticsWorkspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogAnalyticsWorkspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogAnalyticsWorkspaces_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationInsightsEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationInsightsId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SamplingPercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    RetentionInDays = table.Column<int>(type: "integer", nullable: true),
                    DisableIpMasking = table.Column<bool>(type: "boolean", nullable: true),
                    DisableLocalAuth = table.Column<bool>(type: "boolean", nullable: true),
                    IngestionMode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationInsightsEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationInsightsEnvironmentSettings_ApplicationInsights_~",
                        column: x => x.ApplicationInsightsId,
                        principalTable: "ApplicationInsights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogAnalyticsWorkspaceEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogAnalyticsWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RetentionInDays = table.Column<int>(type: "integer", nullable: true),
                    DailyQuotaGb = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogAnalyticsWorkspaceEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogAnalyticsWorkspaceEnvironmentSettings_LogAnalyticsWorksp~",
                        column: x => x.LogAnalyticsWorkspaceId,
                        principalTable: "LogAnalyticsWorkspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInsightsEnvironmentSettings_ApplicationInsightsI~",
                table: "ApplicationInsightsEnvironmentSettings",
                columns: new[] { "ApplicationInsightsId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogAnalyticsWorkspaceEnvironmentSettings_LogAnalyticsWorksp~",
                table: "LogAnalyticsWorkspaceEnvironmentSettings",
                columns: new[] { "LogAnalyticsWorkspaceId", "EnvironmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationInsightsEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "LogAnalyticsWorkspaceEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ApplicationInsights");

            migrationBuilder.DropTable(
                name: "LogAnalyticsWorkspaces");
        }
    }
}
