using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentIntelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentIntelligences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomSubDomainName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentIntelligences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentIntelligences_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentIntelligenceEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentIntelligenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    PublicNetworkAccess = table.Column<string>(type: "text", nullable: true),
                    DisableLocalAuth = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentIntelligenceEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentIntelligenceEnvironmentSettings_DocumentIntelligenc~",
                        column: x => x.DocumentIntelligenceId,
                        principalTable: "DocumentIntelligences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentIntelligenceEnvironmentSettings_DocumentIntelligenc~",
                table: "DocumentIntelligenceEnvironmentSettings",
                columns: new[] { "DocumentIntelligenceId", "EnvironmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentIntelligenceEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "DocumentIntelligences");
        }
    }
}
