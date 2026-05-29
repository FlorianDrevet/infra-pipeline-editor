using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourcePrivateEndpointConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsPrivatized",
                table: "AzureResource",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "PrivateEndpointDnsHubResourceGroupId",
                table: "AzureResource",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateEndpointDnsHubSubscriptionId",
                table: "AzureResource",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateEndpointDnsMode",
                table: "AzureResource",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateEndpointSubnetName",
                table: "AzureResource",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrivateEndpointVirtualNetworkId",
                table: "AzureResource",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateEndpointDnsHubResourceGroupId",
                table: "AzureResource");

            migrationBuilder.DropColumn(
                name: "PrivateEndpointDnsHubSubscriptionId",
                table: "AzureResource");

            migrationBuilder.DropColumn(
                name: "PrivateEndpointDnsMode",
                table: "AzureResource");

            migrationBuilder.DropColumn(
                name: "PrivateEndpointSubnetName",
                table: "AzureResource");

            migrationBuilder.DropColumn(
                name: "PrivateEndpointVirtualNetworkId",
                table: "AzureResource");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrivatized",
                table: "AzureResource",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
