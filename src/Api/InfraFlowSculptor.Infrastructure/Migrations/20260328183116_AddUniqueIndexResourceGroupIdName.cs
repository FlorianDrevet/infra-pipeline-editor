using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexResourceGroupIdName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AzureResource_ResourceGroupId",
                table: "AzureResource");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_ResourceGroupId_Name",
                table: "AzureResource",
                columns: new[] { "ResourceGroupId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AzureResource_ResourceGroupId_Name",
                table: "AzureResource");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_ResourceGroupId",
                table: "AzureResource",
                column: "ResourceGroupId");
        }
    }
}
