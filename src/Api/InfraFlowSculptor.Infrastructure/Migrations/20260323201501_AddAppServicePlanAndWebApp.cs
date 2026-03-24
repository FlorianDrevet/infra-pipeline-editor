using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppServicePlanAndWebApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppServicePlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OsType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServicePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppServicePlans_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppServicePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuntimeStack = table.Column<string>(type: "text", nullable: false),
                    RuntimeVersion = table.Column<string>(type: "text", nullable: false),
                    AlwaysOn = table.Column<bool>(type: "boolean", nullable: false),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebApps_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppServicePlanEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppServicePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "text", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServicePlanEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppServicePlanEnvironmentSettings_AppServicePlans_AppServic~",
                        column: x => x.AppServicePlanId,
                        principalTable: "AppServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebAppEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WebAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "text", nullable: false),
                    AlwaysOn = table.Column<bool>(type: "boolean", nullable: true),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: true),
                    RuntimeStack = table.Column<string>(type: "text", nullable: true),
                    RuntimeVersion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebAppEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebAppEnvironmentSettings_WebApps_WebAppId",
                        column: x => x.WebAppId,
                        principalTable: "WebApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppServicePlanEnvironmentSettings_AppServicePlanId",
                table: "AppServicePlanEnvironmentSettings",
                column: "AppServicePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WebAppEnvironmentSettings_WebAppId",
                table: "WebAppEnvironmentSettings",
                column: "WebAppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppServicePlanEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "WebAppEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "AppServicePlans");

            migrationBuilder.DropTable(
                name: "WebApps");
        }
    }
}
