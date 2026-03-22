using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Projects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntraId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name_FirstName = table.Column<string>(type: "text", nullable: false),
                    Name_LastName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InfrastructureConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultNamingTemplate = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfrastructureConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfrastructureConfigs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_members_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_members_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Environments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: false),
                    Suffix = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Environments_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParameterDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsSecret = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterDefinitions_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceGroup_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceNamingTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceNamingTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceNamingTemplates_InfrastructureConfigs_InfraConfigId",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentParameterValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    EnvironmentDefinitionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentParameterValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvironmentParameterValues_Environments_EnvironmentDefiniti~",
                        column: x => x.EnvironmentDefinitionId,
                        principalTable: "Environments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentTags",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentTags", x => new { x.EnvironmentId, x.Name });
                    table.ForeignKey(
                        name: "FK_EnvironmentTags_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AzureResource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    CustomNameOverride = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AzureResource_ResourceGroup_ResourceGroupId",
                        column: x => x.ResourceGroupId,
                        principalTable: "ResourceGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AzureResourceDependencies",
                columns: table => new
                {
                    DependsOnId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureResourceDependencies", x => new { x.DependsOnId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_AzureResourceDependencies_AzureResource_DependsOnId",
                        column: x => x.DependsOnId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AzureResourceDependencies_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KeyVaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyVaults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyVaults_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedisCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    RedisVersion = table.Column<int>(type: "integer", nullable: false),
                    EnableNonSslPort = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumTlsVersion = table.Column<string>(type: "text", nullable: false),
                    MaxMemoryPolicy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedisCaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedisCaches_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputType = table.Column<int>(type: "integer", nullable: false),
                    InputType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceLinks_AzureResource_SourceResourceId",
                        column: x => x.SourceResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceLinks_AzureResource_TargetResourceId",
                        column: x => x.TargetResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceParameterUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    AzureResourceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceParameterUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceParameterUsages_AzureResource_AzureResourceId",
                        column: x => x.AzureResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResourceParameterUsages_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceParameterUsages_ParameterDefinitions_ParameterId",
                        column: x => x.ParameterId,
                        principalTable: "ParameterDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedIdentityType = table.Column<string>(type: "text", nullable: false),
                    RoleDefinitionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_AzureResource_SourceResourceId",
                        column: x => x.SourceResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_AzureResource_TargetResourceId",
                        column: x => x.TargetResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_AzureResource_ResourceGroupId",
                table: "AzureResource",
                column: "ResourceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResourceDependencies_ResourceId",
                table: "AzureResourceDependencies",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobContainers_StorageAccountId",
                table: "BlobContainers",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentParameterValues_EnvironmentDefinitionId",
                table: "EnvironmentParameterValues",
                column: "EnvironmentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Environments_InfraConfigId",
                table: "Environments",
                column: "InfraConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_InfrastructureConfigs_ProjectId",
                table: "InfrastructureConfigs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_InfraConfigId",
                table: "ParameterDefinitions",
                column: "InfraConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_ProjectId_UserId",
                table: "project_members",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_members_UserId",
                table: "project_members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceGroup_InfraConfigId",
                table: "ResourceGroup",
                column: "InfraConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceLinks_SourceResourceId_TargetResourceId_OutputType_~",
                table: "ResourceLinks",
                columns: new[] { "SourceResourceId", "TargetResourceId", "OutputType", "InputType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceLinks_TargetResourceId",
                table: "ResourceLinks",
                column: "TargetResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceNamingTemplates_InfraConfigId_ResourceType",
                table: "ResourceNamingTemplates",
                columns: new[] { "InfraConfigId", "ResourceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceParameterUsages_AzureResourceId",
                table: "ResourceParameterUsages",
                column: "AzureResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceParameterUsages_ParameterId",
                table: "ResourceParameterUsages",
                column: "ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceParameterUsages_ResourceId_ParameterId_Purpose",
                table: "ResourceParameterUsages",
                columns: new[] { "ResourceId", "ParameterId", "Purpose" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_SourceResourceId",
                table: "RoleAssignments",
                column: "SourceResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_TargetResourceId",
                table: "RoleAssignments",
                column: "TargetResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageQueues_StorageAccountId",
                table: "StorageQueues",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageTables_StorageAccountId",
                table: "StorageTables",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_User_EntraId",
                table: "User",
                column: "EntraId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AzureResourceDependencies");

            migrationBuilder.DropTable(
                name: "BlobContainers");

            migrationBuilder.DropTable(
                name: "EnvironmentParameterValues");

            migrationBuilder.DropTable(
                name: "EnvironmentTags");

            migrationBuilder.DropTable(
                name: "KeyVaults");

            migrationBuilder.DropTable(
                name: "project_members");

            migrationBuilder.DropTable(
                name: "RedisCaches");

            migrationBuilder.DropTable(
                name: "ResourceLinks");

            migrationBuilder.DropTable(
                name: "ResourceNamingTemplates");

            migrationBuilder.DropTable(
                name: "ResourceParameterUsages");

            migrationBuilder.DropTable(
                name: "RoleAssignments");

            migrationBuilder.DropTable(
                name: "StorageQueues");

            migrationBuilder.DropTable(
                name: "StorageTables");

            migrationBuilder.DropTable(
                name: "Environments");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ParameterDefinitions");

            migrationBuilder.DropTable(
                name: "StorageAccounts");

            migrationBuilder.DropTable(
                name: "AzureResource");

            migrationBuilder.DropTable(
                name: "ResourceGroup");

            migrationBuilder.DropTable(
                name: "InfrastructureConfigs");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
