using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineStepOptionsOwnedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CoverageReportPath",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CoverageTool",
                table: "WebApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DependencyScanTool",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_EnableDependencyCache",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_LintCommand",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PublishCodeCoverage",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PublishTestResults",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunBuildValidation",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunDependencyScan",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunLinting",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunSmokeTests",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunSonarAnalysis",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunUnitTests",
                table: "WebApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SmokeTestCommand",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarOrganization",
                table: "WebApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarProjectKey",
                table: "WebApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarServiceConnection",
                table: "WebApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestCommand",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestFramework",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestResultsFormat",
                table: "WebApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestResultsPath",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CoverageReportPath",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CoverageTool",
                table: "FunctionApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DependencyScanTool",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_EnableDependencyCache",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_LintCommand",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PublishCodeCoverage",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PublishTestResults",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunBuildValidation",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunDependencyScan",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunLinting",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunSmokeTests",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunSonarAnalysis",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunUnitTests",
                table: "FunctionApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SmokeTestCommand",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarOrganization",
                table: "FunctionApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarProjectKey",
                table: "FunctionApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarServiceConnection",
                table: "FunctionApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestCommand",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestFramework",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestResultsFormat",
                table: "FunctionApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestResultsPath",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CoverageReportPath",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CoverageTool",
                table: "ContainerApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DependencyScanTool",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_EnableDependencyCache",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_LintCommand",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PublishCodeCoverage",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PublishTestResults",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunBuildValidation",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunDependencyScan",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunLinting",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunSmokeTests",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunSonarAnalysis",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_RunUnitTests",
                table: "ContainerApps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SmokeTestCommand",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarOrganization",
                table: "ContainerApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarProjectKey",
                table: "ContainerApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_SonarServiceConnection",
                table: "ContainerApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestCommand",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestFramework",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestResultsFormat",
                table: "ContainerApps",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_TestResultsPath",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

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
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubnetId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AutoApproval = table.Column<bool>(type: "boolean", nullable: false),
                    PrivateDnsZoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomNetworkInterfaceName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true)
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
                name: "VirtualNetworks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnableDdosProtection = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualNetworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirtualNetworks_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FrontDoorEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FrontDoorId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    TargetResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    PrivateLinkEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false)
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
                    NetworkSecurityGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    Access = table.Column<string>(type: "text", nullable: false),
                    Protocol = table.Column<string>(type: "text", nullable: false),
                    SourceAddressPrefix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DestinationAddressPrefix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourcePortRange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DestinationPortRange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                    PrivateDnsZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    VirtualNetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnableAutoRegistration = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Subnets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VirtualNetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Delegation = table.Column<string>(type: "text", nullable: true),
                    ServiceEndpoints = table.Column<string>(type: "jsonb", nullable: false),
                    PrivateEndpointNetworkPolicies = table.Column<string>(type: "text", nullable: false),
                    NsgId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subnets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subnets_VirtualNetworks_VirtualNetworkId",
                        column: x => x.VirtualNetworkId,
                        principalTable: "VirtualNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VirtualNetworkEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VirtualNetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AddressSpaces = table.Column<string>(type: "jsonb", nullable: false),
                    DnsServers = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualNetworkEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirtualNetworkEnvironmentSettings_VirtualNetworks_VirtualNe~",
                        column: x => x.VirtualNetworkId,
                        principalTable: "VirtualNetworks",
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
                name: "IX_Subnets_VirtualNetworkId",
                table: "Subnets",
                column: "VirtualNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualNetworkEnvironmentSettings_VirtualNetworkId_Environm~",
                table: "VirtualNetworkEnvironmentSettings",
                columns: new[] { "VirtualNetworkId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirtualNetworkLinks_PrivateDnsZoneId",
                table: "VirtualNetworkLinks",
                column: "PrivateDnsZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "Subnets");

            migrationBuilder.DropTable(
                name: "VirtualNetworkEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "VirtualNetworkLinks");

            migrationBuilder.DropTable(
                name: "FrontDoors");

            migrationBuilder.DropTable(
                name: "NetworkSecurityGroups");

            migrationBuilder.DropTable(
                name: "VirtualNetworks");

            migrationBuilder.DropTable(
                name: "PrivateDnsZones");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CoverageReportPath",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CoverageTool",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DependencyScanTool",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_EnableDependencyCache",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_LintCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PublishCodeCoverage",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PublishTestResults",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunBuildValidation",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunDependencyScan",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunLinting",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunSmokeTests",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunSonarAnalysis",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunUnitTests",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SmokeTestCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarOrganization",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarProjectKey",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarServiceConnection",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestFramework",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestResultsFormat",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestResultsPath",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CoverageReportPath",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CoverageTool",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DependencyScanTool",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_EnableDependencyCache",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_LintCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PublishCodeCoverage",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PublishTestResults",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunBuildValidation",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunDependencyScan",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunLinting",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunSmokeTests",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunSonarAnalysis",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunUnitTests",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SmokeTestCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarOrganization",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarProjectKey",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarServiceConnection",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestFramework",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestResultsFormat",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestResultsPath",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CoverageReportPath",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CoverageTool",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DependencyScanTool",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_EnableDependencyCache",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_LintCommand",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PublishCodeCoverage",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PublishTestResults",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunBuildValidation",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunDependencyScan",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunLinting",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunSmokeTests",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunSonarAnalysis",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_RunUnitTests",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SmokeTestCommand",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarOrganization",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarProjectKey",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_SonarServiceConnection",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestCommand",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestFramework",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestResultsFormat",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_TestResultsPath",
                table: "ContainerApps");
        }
    }
}
