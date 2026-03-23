using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurations_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigurationEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SoftDeleteRetentionInDays = table.Column<int>(type: "integer", nullable: true),
                    PurgeProtectionEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    DisableLocalAuth = table.Column<bool>(type: "boolean", nullable: true),
                    PublicNetworkAccess = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurationEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurationEnvironmentSettings_AppConfigurations_AppCo~",
                        column: x => x.AppConfigurationId,
                        principalTable: "AppConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationEnvironmentSettings_AppConfigurationId_Envi~",
                table: "AppConfigurationEnvironmentSettings",
                columns: new[] { "AppConfigurationId", "EnvironmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigurationEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "AppConfigurations");
        }
    }
}
