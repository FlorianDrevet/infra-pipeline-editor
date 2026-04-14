using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleAssignmentTargetCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleAssignments_AzureResource_TargetResourceId",
                table: "RoleAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleAssignments_AzureResource_TargetResourceId",
                table: "RoleAssignments",
                column: "TargetResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleAssignments_AzureResource_TargetResourceId",
                table: "RoleAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleAssignments_AzureResource_TargetResourceId",
                table: "RoleAssignments",
                column: "TargetResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
