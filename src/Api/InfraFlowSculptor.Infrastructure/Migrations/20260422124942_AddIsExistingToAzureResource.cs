using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsExistingToAzureResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExisting",
                table: "AzureResource",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SecureParameterMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecureParameterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PipelineVariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AzureResourceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecureParameterMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecureParameterMappings_AzureResource_AzureResourceId",
                        column: x => x.AzureResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SecureParameterMappings_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecureParameterMappings_ProjectPipelineVariableGroups_Varia~",
                        column: x => x.VariableGroupId,
                        principalTable: "ProjectPipelineVariableGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecureParameterMappings_AzureResourceId",
                table: "SecureParameterMappings",
                column: "AzureResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SecureParameterMappings_ResourceId_SecureParameterName",
                table: "SecureParameterMappings",
                columns: new[] { "ResourceId", "SecureParameterName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecureParameterMappings_VariableGroupId",
                table: "SecureParameterMappings",
                column: "VariableGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecureParameterMappings");

            migrationBuilder.DropColumn(
                name: "IsExisting",
                table: "AzureResource");
        }
    }
}
