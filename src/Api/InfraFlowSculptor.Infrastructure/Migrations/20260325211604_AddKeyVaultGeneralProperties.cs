using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyVaultGeneralProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessTier",
                table: "StorageAccountEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "AllowBlobPublicAccess",
                table: "StorageAccountEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "EnableHttpsTrafficOnly",
                table: "StorageAccountEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "StorageAccountEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "MinimumTlsVersion",
                table: "StorageAccountEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "EnableNonSslPort",
                table: "RedisCacheEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "MinimumTlsVersion",
                table: "RedisCacheEnvironmentSettings");

            migrationBuilder.DropColumn(
                name: "RedisVersion",
                table: "RedisCacheEnvironmentSettings");

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

            migrationBuilder.AddColumn<bool>(
                name: "DisableAccessKeyAuthentication",
                table: "RedisCaches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAadAuth",
                table: "RedisCaches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableNonSslPort",
                table: "RedisCaches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MinimumTlsVersion",
                table: "RedisCaches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RedisVersion",
                table: "RedisCaches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePurgeProtection",
                table: "KeyVaults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableRbacAuthorization",
                table: "KeyVaults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSoftDelete",
                table: "KeyVaults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnabledForDeployment",
                table: "KeyVaults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnabledForDiskEncryption",
                table: "KeyVaults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnabledForTemplateDeployment",
                table: "KeyVaults",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "DisableAccessKeyAuthentication",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "EnableAadAuth",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "EnableNonSslPort",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "MinimumTlsVersion",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "RedisVersion",
                table: "RedisCaches");

            migrationBuilder.DropColumn(
                name: "EnablePurgeProtection",
                table: "KeyVaults");

            migrationBuilder.DropColumn(
                name: "EnableRbacAuthorization",
                table: "KeyVaults");

            migrationBuilder.DropColumn(
                name: "EnableSoftDelete",
                table: "KeyVaults");

            migrationBuilder.DropColumn(
                name: "EnabledForDeployment",
                table: "KeyVaults");

            migrationBuilder.DropColumn(
                name: "EnabledForDiskEncryption",
                table: "KeyVaults");

            migrationBuilder.DropColumn(
                name: "EnabledForTemplateDeployment",
                table: "KeyVaults");

            migrationBuilder.AddColumn<string>(
                name: "AccessTier",
                table: "StorageAccountEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowBlobPublicAccess",
                table: "StorageAccountEnvironmentSettings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableHttpsTrafficOnly",
                table: "StorageAccountEnvironmentSettings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "StorageAccountEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MinimumTlsVersion",
                table: "StorageAccountEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableNonSslPort",
                table: "RedisCacheEnvironmentSettings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MinimumTlsVersion",
                table: "RedisCacheEnvironmentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RedisVersion",
                table: "RedisCacheEnvironmentSettings",
                type: "integer",
                nullable: true);
        }
    }
}
