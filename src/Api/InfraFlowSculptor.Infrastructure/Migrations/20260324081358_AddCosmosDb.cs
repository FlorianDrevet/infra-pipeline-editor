using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCosmosDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CosmosDbAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CosmosDbAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CosmosDbAccounts_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CosmosDbEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CosmosDbId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DatabaseApiType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConsistencyLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxStalenessPrefix = table.Column<int>(type: "integer", nullable: true),
                    MaxIntervalInSeconds = table.Column<int>(type: "integer", nullable: true),
                    EnableAutomaticFailover = table.Column<bool>(type: "boolean", nullable: true),
                    EnableMultipleWriteLocations = table.Column<bool>(type: "boolean", nullable: true),
                    BackupPolicyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EnableFreeTier = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CosmosDbEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CosmosDbEnvironmentSettings_CosmosDbAccounts_CosmosDbId",
                        column: x => x.CosmosDbId,
                        principalTable: "CosmosDbAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CosmosDbEnvironmentSettings_CosmosDbId_EnvironmentName",
                table: "CosmosDbEnvironmentSettings",
                columns: new[] { "CosmosDbId", "EnvironmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CosmosDbEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "CosmosDbAccounts");
        }
    }
}
