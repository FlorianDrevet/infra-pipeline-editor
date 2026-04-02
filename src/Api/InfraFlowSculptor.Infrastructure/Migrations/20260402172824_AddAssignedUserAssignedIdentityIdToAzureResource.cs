using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserAssignedIdentityIdToAzureResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserAssignedIdentityId",
                table: "AzureResource",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_AssignedUserAssignedIdentityId",
                table: "AzureResource",
                column: "AssignedUserAssignedIdentityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResource_AzureResource_AssignedUserAssignedIdentityId",
                table: "AzureResource",
                column: "AssignedUserAssignedIdentityId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AzureResource_AzureResource_AssignedUserAssignedIdentityId",
                table: "AzureResource");

            migrationBuilder.DropIndex(
                name: "IX_AzureResource_AssignedUserAssignedIdentityId",
                table: "AzureResource");

            migrationBuilder.DropColumn(
                name: "AssignedUserAssignedIdentityId",
                table: "AzureResource");
        }
    }
}
