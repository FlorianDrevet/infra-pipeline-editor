using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KeyVault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeyVault_KeyVault_AzureResourceId",
                table: "KeyVault");

            migrationBuilder.DropForeignKey(
                name: "FK_KeyVault_ResourceGroup_ResourceGroupId",
                table: "KeyVault");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KeyVault",
                table: "KeyVault");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "KeyVault");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "KeyVault");

            migrationBuilder.RenameTable(
                name: "KeyVault",
                newName: "AzureResource");

            migrationBuilder.RenameIndex(
                name: "IX_KeyVault_ResourceGroupId",
                table: "AzureResource",
                newName: "IX_AzureResource_ResourceGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_KeyVault_AzureResourceId",
                table: "AzureResource",
                newName: "IX_AzureResource_AzureResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AzureResource",
                table: "AzureResource",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "KeyVaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyVaults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyVaults_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResource_AzureResource_AzureResourceId",
                table: "AzureResource",
                column: "AzureResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResource_ResourceGroup_ResourceGroupId",
                table: "AzureResource",
                column: "ResourceGroupId",
                principalTable: "ResourceGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AzureResource_AzureResource_AzureResourceId",
                table: "AzureResource");

            migrationBuilder.DropForeignKey(
                name: "FK_AzureResource_ResourceGroup_ResourceGroupId",
                table: "AzureResource");

            migrationBuilder.DropTable(
                name: "KeyVaults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AzureResource",
                table: "AzureResource");

            migrationBuilder.RenameTable(
                name: "AzureResource",
                newName: "KeyVault");

            migrationBuilder.RenameIndex(
                name: "IX_AzureResource_ResourceGroupId",
                table: "KeyVault",
                newName: "IX_KeyVault_ResourceGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_AzureResource_AzureResourceId",
                table: "KeyVault",
                newName: "IX_KeyVault_AzureResourceId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "KeyVault",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "KeyVault",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_KeyVault",
                table: "KeyVault",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KeyVault_KeyVault_AzureResourceId",
                table: "KeyVault",
                column: "AzureResourceId",
                principalTable: "KeyVault",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KeyVault_ResourceGroup_ResourceGroupId",
                table: "KeyVault",
                column: "ResourceGroupId",
                principalTable: "ResourceGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
