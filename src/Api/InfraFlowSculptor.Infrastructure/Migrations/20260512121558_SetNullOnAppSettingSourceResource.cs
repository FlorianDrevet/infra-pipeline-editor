using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetNullOnAppSettingSourceResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_AzureResource_SourceResourceId",
                table: "AppSettings",
                column: "SourceResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
