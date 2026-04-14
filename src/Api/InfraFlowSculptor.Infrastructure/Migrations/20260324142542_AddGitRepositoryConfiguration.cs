using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGitRepositoryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GitRepositoryConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<string>(type: "text", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DefaultBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: "main"),
                    BasePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KeyVaultUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SecretName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RepositoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitRepositoryConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitRepositoryConfigurations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GitRepositoryConfigurations_ProjectId",
                table: "GitRepositoryConfigurations",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GitRepositoryConfigurations");
        }
    }
}
