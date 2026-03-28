using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevertUniqueIndexResourceGroupIdName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The unique index (ResourceGroupId, Name) was too strict for TPT inheritance —
            // it prevented different resource types from sharing the same name in a resource group.
            // Revert to the original non-unique index on ResourceGroupId only.
            // Use IF EXISTS because the index may not be present in all database instances.
            migrationBuilder.Sql(
                """DROP INDEX IF EXISTS "IX_AzureResource_ResourceGroupId_Name";""");

            migrationBuilder.Sql(
                """CREATE INDEX IF NOT EXISTS "IX_AzureResource_ResourceGroupId" ON "AzureResource" ("ResourceGroupId");""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
