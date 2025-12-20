using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Environments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_infrastructureconfig_members_InfrastructureConfig_InfraConf~",
                table: "infrastructureconfig_members");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceGroup_InfrastructureConfig_InfraConfigId",
                table: "ResourceGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InfrastructureConfig",
                table: "InfrastructureConfig");

            migrationBuilder.RenameTable(
                name: "InfrastructureConfig",
                newName: "InfrastructureConfigs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InfrastructureConfigs",
                table: "InfrastructureConfigs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Environments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: false),
                    Suffix = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "ParameterDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsSecret = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterDefinitions_InfrastructureConfigs_InfraConfigId",
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
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    EnvironmentDefinitionId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "ResourceParameterUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    AzureResourceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceParameterUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceParameterUsages_AzureResource_AzureResourceId",
                        column: x => x.AzureResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResourceParameterUsages_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceParameterUsages_ParameterDefinitions_ParameterId",
                        column: x => x.ParameterId,
                        principalTable: "ParameterDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentParameterValues_EnvironmentDefinitionId",
                table: "EnvironmentParameterValues",
                column: "EnvironmentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Environments_InfraConfigId",
                table: "Environments",
                column: "InfraConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_InfraConfigId",
                table: "ParameterDefinitions",
                column: "InfraConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceParameterUsages_AzureResourceId",
                table: "ResourceParameterUsages",
                column: "AzureResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceParameterUsages_ParameterId",
                table: "ResourceParameterUsages",
                column: "ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceParameterUsages_ResourceId_ParameterId_Purpose",
                table: "ResourceParameterUsages",
                columns: new[] { "ResourceId", "ParameterId", "Purpose" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_infrastructureconfig_members_InfrastructureConfigs_InfraCon~",
                table: "infrastructureconfig_members",
                column: "InfraConfigId",
                principalTable: "InfrastructureConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceGroup_InfrastructureConfigs_InfraConfigId",
                table: "ResourceGroup",
                column: "InfraConfigId",
                principalTable: "InfrastructureConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_infrastructureconfig_members_InfrastructureConfigs_InfraCon~",
                table: "infrastructureconfig_members");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceGroup_InfrastructureConfigs_InfraConfigId",
                table: "ResourceGroup");

            migrationBuilder.DropTable(
                name: "EnvironmentParameterValues");

            migrationBuilder.DropTable(
                name: "EnvironmentTags");

            migrationBuilder.DropTable(
                name: "ResourceParameterUsages");

            migrationBuilder.DropTable(
                name: "Environments");

            migrationBuilder.DropTable(
                name: "ParameterDefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InfrastructureConfigs",
                table: "InfrastructureConfigs");

            migrationBuilder.RenameTable(
                name: "InfrastructureConfigs",
                newName: "InfrastructureConfig");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InfrastructureConfig",
                table: "InfrastructureConfig",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_infrastructureconfig_members_InfrastructureConfig_InfraConf~",
                table: "infrastructureconfig_members",
                column: "InfraConfigId",
                principalTable: "InfrastructureConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceGroup_InfrastructureConfig_InfraConfigId",
                table: "ResourceGroup",
                column: "InfraConfigId",
                principalTable: "InfrastructureConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
