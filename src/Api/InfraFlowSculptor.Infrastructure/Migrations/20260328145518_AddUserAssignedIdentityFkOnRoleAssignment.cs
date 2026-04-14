using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAssignedIdentityFkOnRoleAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_UserAssignedIdentityId",
                table: "RoleAssignments",
                column: "UserAssignedIdentityId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleAssignments_AzureResource_UserAssignedIdentityId",
                table: "RoleAssignments",
                column: "UserAssignedIdentityId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleAssignments_AzureResource_UserAssignedIdentityId",
                table: "RoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_RoleAssignments_UserAssignedIdentityId",
                table: "RoleAssignments");
        }
    }
}
