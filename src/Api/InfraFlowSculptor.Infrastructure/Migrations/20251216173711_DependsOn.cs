using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DependsOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_AzureResourceId",
                table: "AzureResourceDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnResourceId",
                table: "AzureResourceDependencies");

            migrationBuilder.RenameColumn(
                name: "DependsOnResourceId",
                table: "AzureResourceDependencies",
                newName: "ResourceId");

            migrationBuilder.RenameColumn(
                name: "AzureResourceId",
                table: "AzureResourceDependencies",
                newName: "DependsOnId");

            migrationBuilder.RenameIndex(
                name: "IX_AzureResourceDependencies_DependsOnResourceId",
                table: "AzureResourceDependencies",
                newName: "IX_AzureResourceDependencies_ResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnId",
                table: "AzureResourceDependencies",
                column: "DependsOnId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_ResourceId",
                table: "AzureResourceDependencies",
                column: "ResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnId",
                table: "AzureResourceDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_ResourceId",
                table: "AzureResourceDependencies");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "AzureResourceDependencies",
                newName: "DependsOnResourceId");

            migrationBuilder.RenameColumn(
                name: "DependsOnId",
                table: "AzureResourceDependencies",
                newName: "AzureResourceId");

            migrationBuilder.RenameIndex(
                name: "IX_AzureResourceDependencies_ResourceId",
                table: "AzureResourceDependencies",
                newName: "IX_AzureResourceDependencies_DependsOnResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_AzureResourceId",
                table: "AzureResourceDependencies",
                column: "AzureResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnResourceId",
                table: "AzureResourceDependencies",
                column: "DependsOnResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
