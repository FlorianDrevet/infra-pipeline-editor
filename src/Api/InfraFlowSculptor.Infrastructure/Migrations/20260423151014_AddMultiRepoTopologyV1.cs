using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <summary>
    /// Multi-repo topology V1 migration. Performs four ordered phases inside a single
    /// transaction:
    /// <list type="number">
    ///   <item>Create the <c>ProjectRepositories</c> child table (with unique <c>(ProjectId, Alias)</c> index).</item>
    ///   <item>Backfill one default <c>ProjectRepository</c> per existing <c>GitRepositoryConfiguration</c>.</item>
    ///   <item>Add the inline <c>RepositoryBinding_*</c> columns on <c>InfrastructureConfigs</c> and backfill them to alias <c>"default"</c>.</item>
    ///   <item>Rename <c>RepositoryMode</c> to <c>LayoutPreset</c>, remap legacy enum values, and add the new <c>CommonsStrategy</c> column.</item>
    /// </list>
    /// The four phases were merged into a single migration because EF Core auto-generation
    /// produced a single diff and splitting it into four files would have required artificial
    /// model rollbacks. Atomicity is preserved at deployment time (a migration runs in one transaction).
    /// </summary>
    public partial class AddMultiRepoTopologyV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─────────────────────────────────────────────────────────────────
            // PHASE 1 — Create ProjectRepositories table
            // ─────────────────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProjectRepositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderType = table.Column<string>(type: "text", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RepositoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: "main"),
                    ContentKinds = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRepositories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepositories_ProjectId_Alias",
                table: "ProjectRepositories",
                columns: new[] { "ProjectId", "Alias" },
                unique: true);

            // ─────────────────────────────────────────────────────────────────
            // PHASE 2 — Backfill: 1 GitRepositoryConfiguration -> 1 ProjectRepository ("default")
            // gen_random_uuid() is built-in to PostgreSQL 13+.
            // ─────────────────────────────────────────────────────────────────
            migrationBuilder.Sql(@"
INSERT INTO ""ProjectRepositories"" (""Id"", ""ProjectId"", ""Alias"", ""ProviderType"", ""RepositoryUrl"", ""Owner"", ""RepositoryName"", ""DefaultBranch"", ""ContentKinds"")
SELECT
    gen_random_uuid(),
    grc.""ProjectId"",
    'default',
    grc.""ProviderType"",
    grc.""RepositoryUrl"",
    grc.""Owner"",
    grc.""RepositoryName"",
    grc.""DefaultBranch"",
    'Infrastructure,Pipelines'
FROM ""GitRepositoryConfigurations"" grc
WHERE NOT EXISTS (
    SELECT 1 FROM ""ProjectRepositories"" pr
    WHERE pr.""ProjectId"" = grc.""ProjectId"" AND pr.""Alias"" = 'default'
);
");

            // ─────────────────────────────────────────────────────────────────
            // PHASE 3 — Add RepositoryBinding_* columns and backfill to alias "default"
            // ─────────────────────────────────────────────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "RepositoryBinding_Alias",
                table: "InfrastructureConfigs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryBinding_Branch",
                table: "InfrastructureConfigs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryBinding_InfraPath",
                table: "InfrastructureConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryBinding_PipelinePath",
                table: "InfrastructureConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE ""InfrastructureConfigs"" ic
SET ""RepositoryBinding_Alias"" = 'default'
WHERE EXISTS (
    SELECT 1 FROM ""ProjectRepositories"" pr
    WHERE pr.""ProjectId"" = ic.""ProjectId"" AND pr.""Alias"" = 'default'
);
");

            // ─────────────────────────────────────────────────────────────────
            // PHASE 4 — Rename RepositoryMode -> LayoutPreset, remap legacy values,
            //           add CommonsStrategy column.
            // ─────────────────────────────────────────────────────────────────
            migrationBuilder.RenameColumn(
                name: "RepositoryMode",
                table: "Projects",
                newName: "LayoutPreset");

            // Map legacy RepositoryMode enum values ('MonoRepo'/'MultiRepo') to the new
            // LayoutPreset enum values ('AllInOne'/'SplitInfraCode'/'MultiRepo'/'Custom').
            migrationBuilder.Sql(@"
UPDATE ""Projects"" SET ""LayoutPreset"" = 'AllInOne' WHERE ""LayoutPreset"" = 'MonoRepo';
UPDATE ""Projects"" SET ""LayoutPreset"" = 'MultiRepo' WHERE ""LayoutPreset"" NOT IN ('AllInOne', 'SplitInfraCode', 'MultiRepo', 'Custom');
");

            migrationBuilder.AddColumn<string>(
                name: "CommonsStrategy",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "DuplicatePerRepo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse PHASE 4
            migrationBuilder.DropColumn(
                name: "CommonsStrategy",
                table: "Projects");

            // Remap LayoutPreset enum values back to legacy RepositoryMode values
            // (only MonoRepo / MultiRepo were valid before).
            migrationBuilder.Sql(@"
UPDATE ""Projects"" SET ""LayoutPreset"" = 'MonoRepo' WHERE ""LayoutPreset"" = 'AllInOne';
UPDATE ""Projects"" SET ""LayoutPreset"" = 'MultiRepo' WHERE ""LayoutPreset"" NOT IN ('MonoRepo', 'MultiRepo');
");

            migrationBuilder.RenameColumn(
                name: "LayoutPreset",
                table: "Projects",
                newName: "RepositoryMode");

            // Reverse PHASE 3
            migrationBuilder.DropColumn(
                name: "RepositoryBinding_Alias",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "RepositoryBinding_Branch",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "RepositoryBinding_InfraPath",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "RepositoryBinding_PipelinePath",
                table: "InfrastructureConfigs");

            // Reverse PHASE 2 — drop the backfilled "default" repositories.
            migrationBuilder.Sql(@"DELETE FROM ""ProjectRepositories"" WHERE ""Alias"" = 'default';");

            // Reverse PHASE 1
            migrationBuilder.DropTable(
                name: "ProjectRepositories");
        }
    }
}
