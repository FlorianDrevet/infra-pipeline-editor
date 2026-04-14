using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCrossResourceFKCascadeForProjectDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_KeyVaultResourceId",
                table: "AppConfigurationKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                table: "AppConfigurationKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_KeyVaultResourceId",
                table: "AppConfigurationKeys",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                table: "AppConfigurationKeys",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_KeyVaultResourceId",
                table: "AppConfigurationKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                table: "AppConfigurationKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_KeyVaultResourceId",
                table: "AppConfigurationKeys",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                table: "AppConfigurationKeys",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
