using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConfigEnvironments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnvironmentParameterValues");

            migrationBuilder.DropTable(
                name: "EnvironmentTags");

            migrationBuilder.DropTable(
                name: "Environments");

            migrationBuilder.DropColumn(
                name: "UseProjectEnvironments",
                table: "InfrastructureConfigs");

            migrationBuilder.AddColumn<Guid>(
                name: "KeyVaultResourceId",
                table: "AppSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecretName",
                table: "AppSettings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings");

            migrationBuilder.DropIndex(
                name: "IX_AppSettings_KeyVaultResourceId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "KeyVaultResourceId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "SecretName",
                table: "AppSettings");

            migrationBuilder.AddColumn<bool>(
                name: "UseProjectEnvironments",
                table: "InfrastructureConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Environments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Suffix = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Environments_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentParameterValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentParameterValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvironmentParameterValues_Environments_EnvironmentDefiniti~",
                        column: x => x.EnvironmentDefinitionId,
                        principalTable: "Environments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentTags",
                columns: table => new
                {
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentTags", x => new { x.EnvironmentId, x.Name });
                    table.ForeignKey(
                        name: "FK_EnvironmentTags_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentParameterValues_EnvironmentDefinitionId",
                table: "EnvironmentParameterValues",
                column: "EnvironmentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Environments_InfraConfigId",
                table: "Environments",
                column: "InfraConfigId");
        }
    }
}
