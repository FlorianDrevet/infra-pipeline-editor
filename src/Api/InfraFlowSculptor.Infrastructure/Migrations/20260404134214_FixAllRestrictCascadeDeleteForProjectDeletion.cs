using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAllRestrictCascadeDeleteForProjectDeletion : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnId",
                table: "AzureResourceDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceLinks_AzureResource_SourceResourceId",
                table: "ResourceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceParameterUsages_ParameterDefinitions_ParameterId",
                table: "ResourceParameterUsages");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnId",
                table: "AzureResourceDependencies",
                column: "DependsOnId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceLinks_AzureResource_SourceResourceId",
                table: "ResourceLinks",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceParameterUsages_ParameterDefinitions_ParameterId",
                table: "ResourceParameterUsages",
                column: "ParameterId",
                principalTable: "ParameterDefinitions",
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

            migrationBuilder.DropForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnId",
                table: "AzureResourceDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceLinks_AzureResource_SourceResourceId",
                table: "ResourceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ResourceParameterUsages_ParameterDefinitions_ParameterId",
                table: "ResourceParameterUsages");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_KeyVaultResourceId",
                table: "AppConfigurationKeys",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                table: "AppConfigurationKeys",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AzureResourceDependencies_AzureResource_DependsOnId",
                table: "AzureResourceDependencies",
                column: "DependsOnId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceLinks_AzureResource_SourceResourceId",
                table: "ResourceLinks",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceParameterUsages_ParameterDefinitions_ParameterId",
                table: "ResourceParameterUsages",
                column: "ParameterId",
                principalTable: "ParameterDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
