using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppConfigurationKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigurationKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    KeyVaultResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecretName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SecretValueAssignment = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PipelineVariableName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurationKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeys_AppConfigurations_AppConfigurationId",
                        column: x => x.AppConfigurationId,
                        principalTable: "AppConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeys_AzureResource_KeyVaultResourceId",
                        column: x => x.KeyVaultResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeys_ProjectPipelineVariableGroups_Variable~",
                        column: x => x.VariableGroupId,
                        principalTable: "ProjectPipelineVariableGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigurationKeyEnvironmentValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppConfigurationKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurationKeyEnvironmentValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeyEnvironmentValues_AppConfigurationKeys_A~",
                        column: x => x.AppConfigurationKeyId,
                        principalTable: "AppConfigurationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeyEnvironmentValues_AppConfigurationKeyId_~",
                table: "AppConfigurationKeyEnvironmentValues",
                columns: new[] { "AppConfigurationKeyId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_AppConfigurationId_Key",
                table: "AppConfigurationKeys",
                columns: new[] { "AppConfigurationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_KeyVaultResourceId",
                table: "AppConfigurationKeys",
                column: "KeyVaultResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_VariableGroupId",
                table: "AppConfigurationKeys",
                column: "VariableGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigurationKeyEnvironmentValues");

            migrationBuilder.DropTable(
                name: "AppConfigurationKeys");
        }
    }
}
