using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppConfigurationKeyOutputReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceOutputName",
                table: "AppConfigurationKeys",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceResourceId",
                table: "AppConfigurationKeys",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_SourceResourceId",
                table: "AppConfigurationKeys",
                column: "SourceResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                table: "AppConfigurationKeys",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                table: "AppConfigurationKeys");

            migrationBuilder.DropIndex(
                name: "IX_AppConfigurationKeys_SourceResourceId",
                table: "AppConfigurationKeys");

            migrationBuilder.DropColumn(
                name: "SourceOutputName",
                table: "AppConfigurationKeys");

            migrationBuilder.DropColumn(
                name: "SourceResourceId",
                table: "AppConfigurationKeys");
        }
    }
}
