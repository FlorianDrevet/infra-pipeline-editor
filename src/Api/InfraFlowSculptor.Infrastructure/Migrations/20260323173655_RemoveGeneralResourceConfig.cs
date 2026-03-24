using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGeneralResourceConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessTier",
                table: "StorageAccounts");

            migrationBuilder.DropColumn(
                name: "AllowBlobPublicAccess",
                table: "StorageAccounts");

            migrationBuilder.DropColumn(
                name: "EnableHttpsTrafficOnly",
                table: "StorageAccounts");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "StorageAccounts");

            migrationBuilder.DropColumn(
                name: "MinimumTlsVersion",
                table: "StorageAccounts");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "StorageAccounts");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "EnableNonSslPort",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "MaxMemoryPolicy",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "MinimumTlsVersion",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "RedisVersion",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "KeyVaults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessTier",
                table: "StorageAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AllowBlobPublicAccess",
                table: "StorageAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableHttpsTrafficOnly",
                table: "StorageAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "StorageAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MinimumTlsVersion",
                table: "StorageAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "StorageAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "RedisCaches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EnableNonSslPort",
                table: "RedisCaches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaxMemoryPolicy",
                table: "RedisCaches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MinimumTlsVersion",
                table: "RedisCaches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RedisVersion",
                table: "RedisCaches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "RedisCaches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "KeyVaults",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
