using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSqlServerAndSqlDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SqlDatabases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SqlServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Collation = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlDatabases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SqlDatabases_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SqlServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    AdministratorLogin = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SqlServers_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SqlDatabaseEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SqlDatabaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "text", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    MaxSizeGb = table.Column<int>(type: "integer", nullable: true),
                    ZoneRedundant = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlDatabaseEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SqlDatabaseEnvironmentSettings_SqlDatabases_SqlDatabaseId",
                        column: x => x.SqlDatabaseId,
                        principalTable: "SqlDatabases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SqlServerEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SqlServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "text", nullable: false),
                    MinimalTlsVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlServerEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SqlServerEnvironmentSettings_SqlServers_SqlServerId",
                        column: x => x.SqlServerId,
                        principalTable: "SqlServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SqlDatabaseEnvironmentSettings_SqlDatabaseId",
                table: "SqlDatabaseEnvironmentSettings",
                column: "SqlDatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SqlServerEnvironmentSettings_SqlServerId",
                table: "SqlServerEnvironmentSettings",
                column: "SqlServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SqlDatabaseEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "SqlServerEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "SqlDatabases");

            migrationBuilder.DropTable(
                name: "SqlServers");
        }
    }
}
