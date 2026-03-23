using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TypedEnvironmentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceEnvironmentConfigs_AzureResource_ResourceId",
                table: "ResourceEnvironmentConfigs");

            migrationBuilder.CreateTable(
                name: "KeyVaultEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyVaultId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyVaultEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyVaultEnvironmentSettings_KeyVaults_KeyVaultId",
                        column: x => x.KeyVaultId,
                        principalTable: "KeyVaults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedisCacheEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RedisCacheId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    RedisVersion = table.Column<int>(type: "integer", nullable: true),
                    EnableNonSslPort = table.Column<bool>(type: "boolean", nullable: true),
                    MinimumTlsVersion = table.Column<string>(type: "text", nullable: true),
                    MaxMemoryPolicy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedisCacheEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedisCacheEnvironmentSettings_RedisCaches_RedisCacheId",
                        column: x => x.RedisCacheId,
                        principalTable: "RedisCaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StorageAccountEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    Kind = table.Column<string>(type: "text", nullable: true),
                    AccessTier = table.Column<string>(type: "text", nullable: true),
                    AllowBlobPublicAccess = table.Column<bool>(type: "boolean", nullable: true),
                    EnableHttpsTrafficOnly = table.Column<bool>(type: "boolean", nullable: true),
                    MinimumTlsVersion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageAccountEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageAccountEnvironmentSettings_StorageAccounts_StorageAc~",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyVaultEnvironmentSettings_KeyVaultId_EnvironmentName",
                table: "KeyVaultEnvironmentSettings",
                columns: new[] { "KeyVaultId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedisCacheEnvironmentSettings_RedisCacheId_EnvironmentName",
                table: "RedisCacheEnvironmentSettings",
                columns: new[] { "RedisCacheId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageAccountEnvironmentSettings_StorageAccountId_Environm~",
                table: "StorageAccountEnvironmentSettings",
                columns: new[] { "StorageAccountId", "EnvironmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyVaultEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "RedisCacheEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "StorageAccountEnvironmentSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceEnvironmentConfigs_AzureResource_ResourceId",
                table: "ResourceEnvironmentConfigs",
                column: "ResourceId",
                principalTable: "AzureResource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
