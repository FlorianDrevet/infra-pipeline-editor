using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineVariableGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PipelineVariableGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
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
                name: "PipelineVariableMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineVariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BicepParameterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PipelineVariableMappings");

            migrationBuilder.DropTable(
                name: "PipelineVariableGroups");
        }
    }
}
