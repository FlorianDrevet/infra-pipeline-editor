using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InfrastructureConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InfrastructureConfig",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name_Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfrastructureConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<int>(type: "integer", nullable: false),
                    Name_Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceGroup_InfrastructureConfig_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KeyVault",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<int>(type: "integer", nullable: false),
                    AzureResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Name_Value = table.Column<string>(type: "text", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyVault", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyVault_KeyVault_AzureResourceId",
                        column: x => x.AzureResourceId,
                        principalTable: "KeyVault",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KeyVault_ResourceGroup_ResourceGroupId",
                        column: x => x.ResourceGroupId,
                        principalTable: "ResourceGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyVault_AzureResourceId",
                table: "KeyVault",
                column: "AzureResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyVault_ResourceGroupId",
                table: "KeyVault",
                column: "ResourceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceGroup_InfraConfigId",
                table: "ResourceGroup",
                column: "InfraConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyVault");

            migrationBuilder.DropTable(
                name: "ResourceGroup");

            migrationBuilder.DropTable(
                name: "InfrastructureConfig");
        }
    }
}
