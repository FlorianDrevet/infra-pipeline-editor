using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PipelineVariableMappings");

            migrationBuilder.DropTable(
                name: "ProjectPipelineVariableMappings");

            migrationBuilder.DropTable(
                name: "PipelineVariableGroups");

            migrationBuilder.AddColumn<string>(
                name: "PipelineVariableName",
                table: "AppSettings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VariableGroupId",
                table: "AppSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContainerRegistries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerRegistries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerRegistries_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerRegistryEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerRegistryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AdminUserEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    PublicNetworkAccess = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ZoneRedundancy = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerRegistryEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerRegistryEnvironmentSettings_ContainerRegistries_Co~",
                        column: x => x.ContainerRegistryId,
                        principalTable: "ContainerRegistries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_VariableGroupId",
                table: "AppSettings",
                column: "VariableGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerRegistryEnvironmentSettings_ContainerRegistryId_En~",
                table: "ContainerRegistryEnvironmentSettings",
                columns: new[] { "ContainerRegistryId", "EnvironmentName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_ProjectPipelineVariableGroups_VariableGroupId",
                table: "AppSettings",
                column: "VariableGroupId",
                principalTable: "ProjectPipelineVariableGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_ProjectPipelineVariableGroups_VariableGroupId",
                table: "AppSettings");

            migrationBuilder.DropTable(
                name: "ContainerRegistryEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ContainerRegistries");

            migrationBuilder.DropIndex(
                name: "IX_AppSettings_VariableGroupId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PipelineVariableName",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "VariableGroupId",
                table: "AppSettings");

            migrationBuilder.CreateTable(
                name: "PipelineVariableGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineVariableGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineVariableGroups_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPipelineVariableMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BicepParameterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PipelineVariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPipelineVariableMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPipelineVariableMappings_ProjectPipelineVariableGrou~",
                        column: x => x.VariableGroupId,
                        principalTable: "ProjectPipelineVariableGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineVariableMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BicepParameterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PipelineVariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineVariableMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineVariableMappings_PipelineVariableGroups_VariableGro~",
                        column: x => x.VariableGroupId,
                        principalTable: "PipelineVariableGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineVariableGroups_InfraConfigId_GroupName",
                table: "PipelineVariableGroups",
                columns: new[] { "InfraConfigId", "GroupName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineVariableMappings_VariableGroupId_BicepParameterName",
                table: "PipelineVariableMappings",
                columns: new[] { "VariableGroupId", "BicepParameterName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPipelineVariableMappings_VariableGroupId_BicepParame~",
                table: "ProjectPipelineVariableMappings",
                columns: new[] { "VariableGroupId", "BicepParameterName" },
                unique: true);
        }
    }
}
