using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class existing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecureParameterMappings_AzureResource_AzureResourceId",
                table: "SecureParameterMappings");

            migrationBuilder.DropIndex(
                name: "IX_SecureParameterMappings_AzureResourceId",
                table: "SecureParameterMappings");

            migrationBuilder.DropColumn(
                name: "AzureResourceId",
                table: "SecureParameterMappings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AzureResourceId",
                table: "SecureParameterMappings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecureParameterMappings_AzureResourceId",
                table: "SecureParameterMappings",
                column: "AzureResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SecureParameterMappings_AzureResource_AzureResourceId",
                table: "SecureParameterMappings",
                column: "AzureResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id");
        }
    }
}
