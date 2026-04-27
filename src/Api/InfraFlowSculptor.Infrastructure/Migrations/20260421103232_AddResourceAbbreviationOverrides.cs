using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceAbbreviationOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectResourceAbbreviations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResourceAbbreviations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectResourceAbbreviations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceAbbreviationOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceAbbreviationOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceAbbreviationOverrides_InfrastructureConfigs_InfraCo~",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResourceAbbreviations_ProjectId_ResourceType",
                table: "ProjectResourceAbbreviations",
                columns: new[] { "ProjectId", "ResourceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAbbreviationOverrides_InfraConfigId_ResourceType",
                table: "ResourceAbbreviationOverrides",
                columns: new[] { "InfraConfigId", "ResourceType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectResourceAbbreviations");

            migrationBuilder.DropTable(
                name: "ResourceAbbreviationOverrides");
        }
    }
}
