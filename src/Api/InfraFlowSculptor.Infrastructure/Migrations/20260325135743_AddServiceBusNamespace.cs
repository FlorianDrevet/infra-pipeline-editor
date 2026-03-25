using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceBusNamespace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceBusNamespaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBusNamespaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBusNamespaces_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBusNamespaceEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceBusNamespaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    ZoneRedundant = table.Column<bool>(type: "boolean", nullable: true),
                    DisableLocalAuth = table.Column<bool>(type: "boolean", nullable: true),
                    MinimumTlsVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBusNamespaceEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBusNamespaceEnvironmentSettings_ServiceBusNamespaces~",
                        column: x => x.ServiceBusNamespaceId,
                        principalTable: "ServiceBusNamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBusQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceBusNamespaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBusQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBusQueues_ServiceBusNamespaces_ServiceBusNamespaceId",
                        column: x => x.ServiceBusNamespaceId,
                        principalTable: "ServiceBusNamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBusTopicSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceBusNamespaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    SubscriptionName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBusTopicSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBusTopicSubscriptions_ServiceBusNamespaces_ServiceBu~",
                        column: x => x.ServiceBusNamespaceId,
                        principalTable: "ServiceBusNamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBusNamespaceEnvironmentSettings_ServiceBusNamespaceI~",
                table: "ServiceBusNamespaceEnvironmentSettings",
                columns: new[] { "ServiceBusNamespaceId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBusQueues_ServiceBusNamespaceId_Name",
                table: "ServiceBusQueues",
                columns: new[] { "ServiceBusNamespaceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBusTopicSubscriptions_ServiceBusNamespaceId_TopicNam~",
                table: "ServiceBusTopicSubscriptions",
                columns: new[] { "ServiceBusNamespaceId", "TopicName", "SubscriptionName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceBusNamespaceEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ServiceBusQueues");

            migrationBuilder.DropTable(
                name: "ServiceBusTopicSubscriptions");

            migrationBuilder.DropTable(
                name: "ServiceBusNamespaces");
        }
    }
}
