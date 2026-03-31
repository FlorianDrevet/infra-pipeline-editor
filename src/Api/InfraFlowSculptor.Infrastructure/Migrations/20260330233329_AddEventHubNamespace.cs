using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventHubNamespace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventHubNamespaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHubNamespaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventHubNamespaces_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventHubConsumerGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventHubNamespaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventHubName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ConsumerGroupName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHubConsumerGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventHubConsumerGroups_EventHubNamespaces_EventHubNamespace~",
                        column: x => x.EventHubNamespaceId,
                        principalTable: "EventHubNamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventHubNamespaceEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventHubNamespaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    ZoneRedundant = table.Column<bool>(type: "boolean", nullable: true),
                    DisableLocalAuth = table.Column<bool>(type: "boolean", nullable: true),
                    MinimumTlsVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AutoInflateEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    MaxThroughputUnits = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHubNamespaceEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventHubNamespaceEnvironmentSettings_EventHubNamespaces_Eve~",
                        column: x => x.EventHubNamespaceId,
                        principalTable: "EventHubNamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventHubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventHubNamespaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventHubs_EventHubNamespaces_EventHubNamespaceId",
                        column: x => x.EventHubNamespaceId,
                        principalTable: "EventHubNamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventHubConsumerGroups_EventHubNamespaceId_EventHubName_Con~",
                table: "EventHubConsumerGroups",
                columns: new[] { "EventHubNamespaceId", "EventHubName", "ConsumerGroupName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventHubNamespaceEnvironmentSettings_EventHubNamespaceId_En~",
                table: "EventHubNamespaceEnvironmentSettings",
                columns: new[] { "EventHubNamespaceId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventHubs_EventHubNamespaceId_Name",
                table: "EventHubs",
                columns: new[] { "EventHubNamespaceId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventHubConsumerGroups");

            migrationBuilder.DropTable(
                name: "EventHubNamespaceEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "EventHubs");

            migrationBuilder.DropTable(
                name: "EventHubNamespaces");
        }
    }
}
