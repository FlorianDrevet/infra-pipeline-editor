using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectEnvironmentsAndNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultNamingTemplate",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseProjectEnvironments",
                table: "InfrastructureConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseProjectNamingConventions",
                table: "InfrastructureConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "ProjectEnvironments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_ProjectEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectEnvironments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectResourceNamingTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResourceNamingTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectResourceNamingTemplates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEnvironmentTags",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEnvironmentTags", x => new { x.EnvironmentId, x.Name });
                    table.ForeignKey(
                        name: "FK_ProjectEnvironmentTags_ProjectEnvironments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "ProjectEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEnvironments_ProjectId",
                table: "ProjectEnvironments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResourceNamingTemplates_ProjectId_ResourceType",
                table: "ProjectResourceNamingTemplates",
                columns: new[] { "ProjectId", "ResourceType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectEnvironmentTags");

            migrationBuilder.DropTable(
                name: "ProjectResourceNamingTemplates");

            migrationBuilder.DropTable(
                name: "ProjectEnvironments");

            migrationBuilder.DropColumn(
                name: "DefaultNamingTemplate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UseProjectEnvironments",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "UseProjectNamingConventions",
                table: "InfrastructureConfigs");
        }
    }
}
