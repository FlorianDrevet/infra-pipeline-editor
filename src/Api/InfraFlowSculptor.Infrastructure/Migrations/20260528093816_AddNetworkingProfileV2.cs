using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNetworkingProfileV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrontDoorEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "FrontDoorOrigins");

            migrationBuilder.DropTable(
                name: "NsgRules");

            migrationBuilder.DropTable(
                name: "PrivateEndpointConfigs");

            migrationBuilder.DropTable(
                name: "VirtualNetworkLinks");

            migrationBuilder.DropTable(
                name: "FrontDoors");

            migrationBuilder.DropTable(
                name: "NetworkSecurityGroups");

            migrationBuilder.DropTable(
                name: "PrivateDnsZones");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivatized",
                table: "AzureResource",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "NetworkingProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VnetSource = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VnetExistingResourceId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VnetCreateNewAddressSpace = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VnetPeSubnetName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    VnetPeSubnetAddressPrefix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DnsMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DnsHubResourceGroupId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DnsHubSubscriptionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkingProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkingProfiles_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkingProfileEnvironmentOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NetworkingProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VnetSourceOverride = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VnetExistingResourceIdOverride = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VnetCreateNewAddressSpaceOverride = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VnetPeSubnetNameOverride = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    VnetPeSubnetAddressPrefixOverride = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DnsModeOverride = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DnsHubResourceGroupIdOverride = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DnsHubSubscriptionIdOverride = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkingProfileEnvironmentOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkingProfileEnvironmentOverrides_NetworkingProfiles_Ne~",
                        column: x => x.NetworkingProfileId,
                        principalTable: "NetworkingProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NetworkingProfileEnvironmentOverrides_NetworkingProfileId_E~",
                table: "NetworkingProfileEnvironmentOverrides",
                columns: new[] { "NetworkingProfileId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NetworkingProfiles_InfraConfigId",
                table: "NetworkingProfiles",
                column: "InfraConfigId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NetworkingProfileEnvironmentOverrides");

            migrationBuilder.DropTable(
                name: "NetworkingProfiles");

            migrationBuilder.DropColumn(
                name: "IsPrivatized",
                table: "AzureResource");

            migrationBuilder.CreateTable(
                name: "FrontDoors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WafPolicyEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrontDoors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrontDoors_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkSecurityGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkSecurityGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkSecurityGroups_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivateDnsZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateDnsZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateDnsZones_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivateEndpointConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoApproval = table.Column<bool>(type: "boolean", nullable: false),
                    CustomNetworkInterfaceName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    GroupId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrivateDnsZoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubnetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateEndpointConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateEndpointConfigs_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FrontDoorEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FrontDoorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrontDoorEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrontDoorEnvironmentSettings_FrontDoors_FrontDoorId",
                        column: x => x.FrontDoorId,
                        principalTable: "FrontDoors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FrontDoorOrigins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FrontDoorId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    PrivateLinkEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TargetResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrontDoorOrigins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrontDoorOrigins_FrontDoors_FrontDoorId",
                        column: x => x.FrontDoorId,
                        principalTable: "FrontDoors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NsgRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Access = table.Column<string>(type: "text", nullable: false),
                    DestinationAddressPrefix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DestinationPortRange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    NetworkSecurityGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Protocol = table.Column<string>(type: "text", nullable: false),
                    SourceAddressPrefix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourcePortRange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NsgRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NsgRules_NetworkSecurityGroups_NetworkSecurityGroupId",
                        column: x => x.NetworkSecurityGroupId,
                        principalTable: "NetworkSecurityGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VirtualNetworkLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnableAutoRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    PrivateDnsZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    VirtualNetworkId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualNetworkLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirtualNetworkLinks_PrivateDnsZones_PrivateDnsZoneId",
                        column: x => x.PrivateDnsZoneId,
                        principalTable: "PrivateDnsZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FrontDoorEnvironmentSettings_FrontDoorId_EnvironmentName",
                table: "FrontDoorEnvironmentSettings",
                columns: new[] { "FrontDoorId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrontDoorOrigins_FrontDoorId",
                table: "FrontDoorOrigins",
                column: "FrontDoorId");

            migrationBuilder.CreateIndex(
                name: "IX_NsgRules_NetworkSecurityGroupId_Priority_Direction",
                table: "NsgRules",
                columns: new[] { "NetworkSecurityGroupId", "Priority", "Direction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateEndpointConfigs_ResourceId",
                table: "PrivateEndpointConfigs",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualNetworkLinks_PrivateDnsZoneId",
                table: "VirtualNetworkLinks",
                column: "PrivateDnsZoneId");
        }
    }
}
