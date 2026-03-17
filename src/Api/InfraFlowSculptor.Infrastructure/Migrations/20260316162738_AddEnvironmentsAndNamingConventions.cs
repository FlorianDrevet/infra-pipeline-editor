using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnvironmentsAndNamingConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultNamingTemplate",
                table: "InfrastructureConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomNameOverride",
                table: "AzureResource",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResourceNamingTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceNamingTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceNamingTemplates_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceNamingTemplates_InfraConfigId_ResourceType",
                table: "ResourceNamingTemplates",
                columns: new[] { "InfraConfigId", "ResourceType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceNamingTemplates");

            migrationBuilder.DropColumn(
                name: "DefaultNamingTemplate",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "CustomNameOverride",
                table: "AzureResource");
        }
    }
}
