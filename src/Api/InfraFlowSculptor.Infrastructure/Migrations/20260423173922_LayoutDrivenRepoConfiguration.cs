using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LayoutDrivenRepoConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommonsStrategy",
                table: "Projects");

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

            migrationBuilder.AddColumn<string>(
                name: "LayoutMode",
                table: "InfrastructureConfigs",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InfraConfigRepositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfrastructureConfigId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_InfraConfigRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfraConfigRepositories_InfrastructureConfigs_Infrastructur~",
                        column: x => x.InfrastructureConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InfraConfigRepositories_InfrastructureConfigId_Alias",
                table: "InfraConfigRepositories",
                columns: new[] { "InfrastructureConfigId", "Alias" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InfraConfigRepositories");

            migrationBuilder.DropColumn(
                name: "LayoutMode",
                table: "InfrastructureConfigs");

            migrationBuilder.AddColumn<string>(
                name: "CommonsStrategy",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "DuplicatePerRepo");

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
        }
    }
}
