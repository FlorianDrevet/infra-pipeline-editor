using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVnetEnvironmentDdosProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableDdosProtection",
                table: "VirtualNetworks");

            migrationBuilder.AddColumn<bool>(
                name: "EnableDdosProtection",
                table: "VirtualNetworkEnvironmentSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableDdosProtection",
                table: "VirtualNetworkEnvironmentSettings");

            migrationBuilder.AddColumn<bool>(
                name: "EnableDdosProtection",
                table: "VirtualNetworks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
