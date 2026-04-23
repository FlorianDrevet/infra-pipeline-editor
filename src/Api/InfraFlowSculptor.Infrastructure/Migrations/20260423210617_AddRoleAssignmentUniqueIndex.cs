using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAssignmentUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings");

            migrationBuilder.DropIndex(
                name: "IX_RoleAssignments_SourceResourceId",
                table: "RoleAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_SourceResourceId_TargetResourceId_UserAssig~",
                table: "RoleAssignments",
                columns: new[] { "SourceResourceId", "TargetResourceId", "UserAssignedIdentityId", "RoleDefinitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings");

            migrationBuilder.DropIndex(
                name: "IX_RoleAssignments_SourceResourceId_TargetResourceId_UserAssig~",
                table: "RoleAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_SourceResourceId",
                table: "RoleAssignments",
                column: "SourceResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
