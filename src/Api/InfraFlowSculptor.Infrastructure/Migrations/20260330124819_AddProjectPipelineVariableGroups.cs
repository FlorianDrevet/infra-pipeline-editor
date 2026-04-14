using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectPipelineVariableGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectPipelineVariableGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPipelineVariableGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPipelineVariableGroups_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPipelineVariableMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineVariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BicepParameterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPipelineVariableGroups_ProjectId_GroupName",
                table: "ProjectPipelineVariableGroups",
                columns: new[] { "ProjectId", "GroupName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPipelineVariableMappings_VariableGroupId_BicepParame~",
                table: "ProjectPipelineVariableMappings",
                columns: new[] { "VariableGroupId", "BicepParameterName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectPipelineVariableMappings");

            migrationBuilder.DropTable(
                name: "ProjectPipelineVariableGroups");
        }
    }
}
