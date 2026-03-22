using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dev-only: wipe all existing data that would conflict with the new schema
            migrationBuilder.Sql("""
                TRUNCATE TABLE "InfrastructureConfigs" CASCADE;
                TRUNCATE TABLE "infrastructureconfig_members" CASCADE;
                DROP TABLE IF EXISTS "project_members";
                DROP TABLE IF EXISTS "Projects";
                ALTER TABLE "InfrastructureConfigs" DROP COLUMN IF EXISTS "ProjectId";
                """);

            migrationBuilder.DropTable(
                name: "infrastructureconfig_members");

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_members_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_members_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "InfrastructureConfigs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_InfrastructureConfigs_ProjectId",
                table: "InfrastructureConfigs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_ProjectId_UserId",
                table: "project_members",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_members_UserId",
                table: "project_members",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InfrastructureConfigs_Projects_ProjectId",
                table: "InfrastructureConfigs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InfrastructureConfigs_Projects_ProjectId",
                table: "InfrastructureConfigs");

            migrationBuilder.DropTable(
                name: "project_members");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_InfrastructureConfigs_ProjectId",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "InfrastructureConfigs");

            migrationBuilder.CreateTable(
                name: "infrastructureconfig_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_infrastructureconfig_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_infrastructureconfig_members_InfrastructureConfigs_InfraCon~",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_infrastructureconfig_members_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_infrastructureconfig_members_InfraConfigId_UserId",
                table: "infrastructureconfig_members",
                columns: new[] { "InfraConfigId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_infrastructureconfig_members_UserId",
                table: "infrastructureconfig_members",
                column: "UserId");
        }
    }
}
