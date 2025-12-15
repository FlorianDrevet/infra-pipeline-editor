using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AzureResourceDependsOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AzureResource_AzureResource_AzureResourceId",
                table: "AzureResource");

            migrationBuilder.DropIndex(
                name: "IX_AzureResource_AzureResourceId",
                table: "AzureResource");

            migrationBuilder.DropColumn(
                name: "AzureResourceId",
                table: "AzureResource");

            migrationBuilder.CreateTable(
                name: "AzureResourceDependencies",
                columns: table => new
                {
                    AzureResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DependsOnResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureResourceDependencies", x => new { x.AzureResourceId, x.DependsOnResourceId });
                    table.ForeignKey(
                        name: "FK_AzureResourceDependencies_AzureResource_AzureResourceId",
                        column: x => x.AzureResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AzureResourceDependencies_AzureResource_DependsOnResourceId",
                        column: x => x.DependsOnResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AzureResourceDependencies_DependsOnResourceId",
                table: "AzureResourceDependencies",
                column: "DependsOnResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AzureResourceDependencies");

            migrationBuilder.AddColumn<Guid>(
                name: "AzureResourceId",
                table: "AzureResource",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_AzureResourceId",
                table: "AzureResource",
                column: "AzureResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResource_AzureResource_AzureResourceId",
                table: "AzureResource",
                column: "AzureResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id");
        }
    }
}
