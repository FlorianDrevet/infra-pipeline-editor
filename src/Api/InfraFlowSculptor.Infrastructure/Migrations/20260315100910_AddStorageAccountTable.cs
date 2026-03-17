using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorageAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    AccessTier = table.Column<string>(type: "text", nullable: false),
                    AllowBlobPublicAccess = table.Column<bool>(type: "boolean", nullable: false),
                    EnableHttpsTrafficOnly = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumTlsVersion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageAccounts_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlobContainers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PublicAccess = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlobContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlobContainers_StorageAccounts_StorageAccountId",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StorageQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageQueues_StorageAccounts_StorageAccountId",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StorageTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageTables_StorageAccounts_StorageAccountId",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlobContainers_StorageAccountId",
                table: "BlobContainers",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageQueues_StorageAccountId",
                table: "StorageQueues",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageTables_StorageAccountId",
                table: "StorageTables",
                column: "StorageAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlobContainers");

            migrationBuilder.DropTable(
                name: "StorageQueues");

            migrationBuilder.DropTable(
                name: "StorageTables");

            migrationBuilder.DropTable(
                name: "StorageAccounts");
        }
    }
}
