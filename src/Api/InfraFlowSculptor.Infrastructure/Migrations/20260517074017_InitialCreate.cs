using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalAccessTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalAccessTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DefaultNamingTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LayoutPreset = table.Column<string>(type: "text", nullable: false, defaultValue: "MultiRepo"),
                    AgentPoolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
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
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultNamingTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UseProjectNamingConventions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AppPipelineMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Isolated"),
                    LayoutMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
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
                name: "ProjectEnvironments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Prefix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AzureResourceManagerConnection = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectEnvironments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPipelineVariableGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPipelineVariableGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPipelineVariableGroups_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRepositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderType = table.Column<string>(type: "text", nullable: true),
                    RepositoryUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RepositoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContentKinds = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRepositories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectResourceAbbreviations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResourceAbbreviations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectResourceAbbreviations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectResourceNamingTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Template = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResourceNamingTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectResourceNamingTemplates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTags",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTags", x => new { x.ProjectId, x.Name });
                    table.ForeignKey(
                        name: "FK_ProjectTags_Projects_ProjectId",
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
                name: "CrossConfigResourceReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossConfigResourceReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrossConfigResourceReferences_InfrastructureConfigs_InfraCo~",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InfraConfigRepositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfrastructureConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderType = table.Column<string>(type: "text", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RepositoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: "main"),
                    ContentKinds = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfraConfigRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfraConfigRepositories_InfrastructureConfigs_Infrastructur~",
                        column: x => x.InfrastructureConfigId,
                        principalTable: "InfrastructureConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InfrastructureConfigTags",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InfrastructureConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfrastructureConfigTags", x => new { x.InfrastructureConfigId, x.Name });
                    table.ForeignKey(
                        name: "FK_InfrastructureConfigTags_InfrastructureConfigs_Infrastructu~",
                        column: x => x.InfrastructureConfigId,
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
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsSecret = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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
                name: "ResourceAbbreviationOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceAbbreviationOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceAbbreviationOverrides_InfrastructureConfigs_InfraCo~",
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
                    Name = table.Column<string>(type: "character varying(90)", maxLength: 90, nullable: false),
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
                    Template = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
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
                name: "ProjectEnvironmentTags",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEnvironmentTags", x => new { x.EnvironmentId, x.Name });
                    table.ForeignKey(
                        name: "FK_ProjectEnvironmentTags_ProjectEnvironments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "ProjectEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AzureResource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResourceGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    CustomNameOverride = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    IsExisting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AssignedUserAssignedIdentityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AzureResource_AzureResource_AssignedUserAssignedIdentityId",
                        column: x => x.AssignedUserAssignedIdentityId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AzureResource_ResourceGroup_ResourceGroupId",
                        column: x => x.ResourceGroupId,
                        principalTable: "ResourceGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurations_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogAnalyticsWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationInsights_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppServicePlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OsType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServicePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppServicePlans_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceOutputName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    KeyVaultResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecretName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SecretValueAssignment = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PipelineVariableName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSettings_AzureResource_KeyVaultResourceId",
                        column: x => x.KeyVaultResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppSettings_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppSettings_AzureResource_SourceResourceId",
                        column: x => x.SourceResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppSettings_ProjectPipelineVariableGroups_VariableGroupId",
                        column: x => x.VariableGroupId,
                        principalTable: "ProjectPipelineVariableGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AzureResourceDependencies_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerAppEnvironments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogAnalyticsWorkspaceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerAppEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerAppEnvironments_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerAppEnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerRegistryId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcrAuthMode = table.Column<string>(type: "text", nullable: true),
                    AcrPullIdentityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DockerImageName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DockerImageValidated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DockerfilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApplicationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_RunUnitTests = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_TestCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_TestFramework = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PipelineStepOptions_TestResultsFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PipelineStepOptions_TestResultsPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_PublishTestResults = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_PublishCodeCoverage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_CoverageTool = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PipelineStepOptions_CoverageReportPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_RunSonarAnalysis = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_SonarProjectKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_SonarOrganization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_SonarServiceConnection = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_RunLinting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_LintCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_RunDependencyScan = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_DependencyScanTool = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PipelineStepOptions_RunBuildValidation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_EnableDependencyCache = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_RunSmokeTests = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_SmokeTestCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerApps_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerRegistries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerRegistries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerRegistries_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "CustomDomains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DomainName = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    CertificateMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "ManagedCertificate"),
                    KeyVaultUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ManagedIdentityResourceId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DnsValidationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomDomains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomDomains_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "FunctionApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppServicePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuntimeStack = table.Column<string>(type: "text", nullable: false),
                    RuntimeVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: false),
                    DeploymentMode = table.Column<string>(type: "text", nullable: false),
                    ContainerRegistryId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcrAuthMode = table.Column<string>(type: "text", nullable: true),
                    AcrPullIdentityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DockerImageName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DockerImageValidated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DockerfilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceCodePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BuildCommand = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApplicationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_RunUnitTests = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_TestCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_TestFramework = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PipelineStepOptions_TestResultsFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PipelineStepOptions_TestResultsPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_PublishTestResults = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_PublishCodeCoverage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_CoverageTool = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PipelineStepOptions_CoverageReportPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_RunSonarAnalysis = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_SonarProjectKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_SonarOrganization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_SonarServiceConnection = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_RunLinting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_LintCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_RunDependencyScan = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_DependencyScanTool = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PipelineStepOptions_RunBuildValidation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_EnableDependencyCache = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_RunSmokeTests = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_SmokeTestCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunctionApps_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KeyVaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnableRbacAuthorization = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledForDeployment = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledForDiskEncryption = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledForTemplateDeployment = table.Column<bool>(type: "boolean", nullable: false),
                    EnablePurgeProtection = table.Column<bool>(type: "boolean", nullable: false),
                    EnableSoftDelete = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "LogAnalyticsWorkspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogAnalyticsWorkspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogAnalyticsWorkspaces_AzureResource_Id",
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
                name: "RedisCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RedisVersion = table.Column<int>(type: "integer", nullable: true),
                    EnableNonSslPort = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MinimumTlsVersion = table.Column<string>(type: "text", nullable: true),
                    DisableAccessKeyAuthentication = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EnableAadAuth = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedIdentityType = table.Column<string>(type: "text", nullable: false),
                    RoleDefinitionId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    UserAssignedIdentityId = table.Column<Guid>(type: "uuid", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_AzureResource_UserAssignedIdentityId",
                        column: x => x.UserAssignedIdentityId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SecureParameterMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecureParameterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PipelineVariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecureParameterMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecureParameterMappings_AzureResource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecureParameterMappings_ProjectPipelineVariableGroups_Varia~",
                        column: x => x.VariableGroupId,
                        principalTable: "ProjectPipelineVariableGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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
                name: "StorageAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "UserAssignedIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssignedIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAssignedIdentities_AzureResource_Id",
                        column: x => x.Id,
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
                name: "WebApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppServicePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuntimeStack = table.Column<string>(type: "text", nullable: false),
                    RuntimeVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AlwaysOn = table.Column<bool>(type: "boolean", nullable: false),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: false),
                    DeploymentMode = table.Column<string>(type: "text", nullable: false),
                    ContainerRegistryId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcrAuthMode = table.Column<string>(type: "text", nullable: true),
                    AcrPullIdentityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DockerImageName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DockerImageValidated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DockerfilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceCodePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BuildCommand = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApplicationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_RunUnitTests = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_TestCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_TestFramework = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PipelineStepOptions_TestResultsFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PipelineStepOptions_TestResultsPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_PublishTestResults = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_PublishCodeCoverage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_CoverageTool = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PipelineStepOptions_CoverageReportPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_RunSonarAnalysis = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_SonarProjectKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_SonarOrganization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_SonarServiceConnection = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PipelineStepOptions_RunLinting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_LintCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PipelineStepOptions_RunDependencyScan = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_DependencyScanTool = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PipelineStepOptions_RunBuildValidation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_EnableDependencyCache = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_RunSmokeTests = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PipelineStepOptions_SmokeTestCommand = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebApps_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigurationEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SoftDeleteRetentionInDays = table.Column<int>(type: "integer", nullable: true),
                    PurgeProtectionEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    DisableLocalAuth = table.Column<bool>(type: "boolean", nullable: true),
                    PublicNetworkAccess = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurationEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurationEnvironmentSettings_AppConfigurations_AppCo~",
                        column: x => x.AppConfigurationId,
                        principalTable: "AppConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigurationKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    KeyVaultResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecretName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SecretValueAssignment = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VariableGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PipelineVariableName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceOutputName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurationKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeys_AppConfigurations_AppConfigurationId",
                        column: x => x.AppConfigurationId,
                        principalTable: "AppConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeys_AzureResource_KeyVaultResourceId",
                        column: x => x.KeyVaultResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeys_AzureResource_SourceResourceId",
                        column: x => x.SourceResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeys_ProjectPipelineVariableGroups_Variable~",
                        column: x => x.VariableGroupId,
                        principalTable: "ProjectPipelineVariableGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationInsightsEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationInsightsId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SamplingPercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    RetentionInDays = table.Column<int>(type: "integer", nullable: true),
                    DisableIpMasking = table.Column<bool>(type: "boolean", nullable: true),
                    DisableLocalAuth = table.Column<bool>(type: "boolean", nullable: true),
                    IngestionMode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationInsightsEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationInsightsEnvironmentSettings_ApplicationInsights_~",
                        column: x => x.ApplicationInsightsId,
                        principalTable: "ApplicationInsights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppServicePlanEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppServicePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServicePlanEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppServicePlanEnvironmentSettings_AppServicePlans_AppServic~",
                        column: x => x.AppServicePlanId,
                        principalTable: "AppServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppSettingEnvironmentValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppSettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettingEnvironmentValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSettingEnvironmentValues_AppSettings_AppSettingId",
                        column: x => x.AppSettingId,
                        principalTable: "AppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerAppEnvironmentEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerAppEnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WorkloadProfileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InternalLoadBalancerEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    ZoneRedundancyEnabled = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerAppEnvironmentEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerAppEnvironmentEnvironmentSettings_ContainerAppEnvi~",
                        column: x => x.ContainerAppEnvironmentId,
                        principalTable: "ContainerAppEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerAppEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CpuCores = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MemoryGi = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MinReplicas = table.Column<int>(type: "integer", nullable: true),
                    MaxReplicas = table.Column<int>(type: "integer", nullable: true),
                    IngressEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    IngressTargetPort = table.Column<int>(type: "integer", nullable: true),
                    IngressExternal = table.Column<bool>(type: "boolean", nullable: true),
                    TransportMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ReadinessProbePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReadinessProbePort = table.Column<int>(type: "integer", nullable: true),
                    LivenessProbePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LivenessProbePort = table.Column<int>(type: "integer", nullable: true),
                    StartupProbePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartupProbePort = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerAppEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerAppEnvironmentSettings_ContainerApps_ContainerAppId",
                        column: x => x.ContainerAppId,
                        principalTable: "ContainerApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerRegistryEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerRegistryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AdminUserEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    PublicNetworkAccess = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ZoneRedundancy = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerRegistryEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerRegistryEnvironmentSettings_ContainerRegistries_Co~",
                        column: x => x.ContainerRegistryId,
                        principalTable: "ContainerRegistries",
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
                name: "FunctionAppEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FunctionAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: true),
                    MaxInstanceCount = table.Column<int>(type: "integer", nullable: true),
                    DockerImageTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionAppEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunctionAppEnvironmentSettings_FunctionApps_FunctionAppId",
                        column: x => x.FunctionAppId,
                        principalTable: "FunctionApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "LogAnalyticsWorkspaceEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogAnalyticsWorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RetentionInDays = table.Column<int>(type: "integer", nullable: true),
                    DailyQuotaGb = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogAnalyticsWorkspaceEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogAnalyticsWorkspaceEnvironmentSettings_LogAnalyticsWorksp~",
                        column: x => x.LogAnalyticsWorkspaceId,
                        principalTable: "LogAnalyticsWorkspaces",
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
                name: "RedisCacheEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RedisCacheId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "SqlDatabaseEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SqlDatabaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "BlobContainers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
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
                name: "BlobLifecycleRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContainerNames = table.Column<string>(type: "jsonb", nullable: false),
                    TimeToLiveInDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlobLifecycleRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlobLifecycleRules_StorageAccounts_StorageAccountId",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StorageAccountCorsRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceType = table.Column<string>(type: "text", nullable: false),
                    MaxAgeInSeconds = table.Column<int>(type: "integer", nullable: false),
                    AllowedHeaders = table.Column<List<string>>(type: "text[]", nullable: false),
                    AllowedMethods = table.Column<List<string>>(type: "text[]", nullable: false),
                    AllowedOrigins = table.Column<List<string>>(type: "text[]", nullable: false),
                    ExposedHeaders = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageAccountCorsRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageAccountCorsRules_StorageAccounts_StorageAccountId",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
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
                    Sku = table.Column<string>(type: "text", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "StorageQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false)
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
                    Name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "WebAppEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WebAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AlwaysOn = table.Column<bool>(type: "boolean", nullable: true),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: true),
                    DockerImageTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebAppEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebAppEnvironmentSettings_WebApps_WebAppId",
                        column: x => x.WebAppId,
                        principalTable: "WebApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigurationKeyEnvironmentValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppConfigurationKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurationKeyEnvironmentValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigurationKeyEnvironmentValues_AppConfigurationKeys_A~",
                        column: x => x.AppConfigurationKeyId,
                        principalTable: "AppConfigurationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationEnvironmentSettings_AppConfigurationId_Envi~",
                table: "AppConfigurationEnvironmentSettings",
                columns: new[] { "AppConfigurationId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeyEnvironmentValues_AppConfigurationKeyId_~",
                table: "AppConfigurationKeyEnvironmentValues",
                columns: new[] { "AppConfigurationKeyId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_AppConfigurationId_Key",
                table: "AppConfigurationKeys",
                columns: new[] { "AppConfigurationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_KeyVaultResourceId",
                table: "AppConfigurationKeys",
                column: "KeyVaultResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_SourceResourceId",
                table: "AppConfigurationKeys",
                column: "SourceResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurationKeys_VariableGroupId",
                table: "AppConfigurationKeys",
                column: "VariableGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInsightsEnvironmentSettings_ApplicationInsightsI~",
                table: "ApplicationInsightsEnvironmentSettings",
                columns: new[] { "ApplicationInsightsId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppServicePlanEnvironmentSettings_AppServicePlanId",
                table: "AppServicePlanEnvironmentSettings",
                column: "AppServicePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettingEnvironmentValues_AppSettingId_EnvironmentName",
                table: "AppSettingEnvironmentValues",
                columns: new[] { "AppSettingId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_KeyVaultResourceId",
                table: "AppSettings",
                column: "KeyVaultResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_ResourceId_Name",
                table: "AppSettings",
                columns: new[] { "ResourceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_SourceResourceId",
                table: "AppSettings",
                column: "SourceResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_VariableGroupId",
                table: "AppSettings",
                column: "VariableGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_AssignedUserAssignedIdentityId",
                table: "AzureResource",
                column: "AssignedUserAssignedIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_ResourceGroupId",
                table: "AzureResource",
                column: "ResourceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResource_ResourceType",
                table: "AzureResource",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_AzureResourceDependencies_ResourceId",
                table: "AzureResourceDependencies",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobContainers_StorageAccountId",
                table: "BlobContainers",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobLifecycleRules_StorageAccountId",
                table: "BlobLifecycleRules",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerAppEnvironmentEnvironmentSettings_ContainerAppEnvi~",
                table: "ContainerAppEnvironmentEnvironmentSettings",
                columns: new[] { "ContainerAppEnvironmentId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContainerAppEnvironmentSettings_ContainerAppId_EnvironmentN~",
                table: "ContainerAppEnvironmentSettings",
                columns: new[] { "ContainerAppId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContainerRegistryEnvironmentSettings_ContainerRegistryId_En~",
                table: "ContainerRegistryEnvironmentSettings",
                columns: new[] { "ContainerRegistryId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CosmosDbEnvironmentSettings_CosmosDbId_EnvironmentName",
                table: "CosmosDbEnvironmentSettings",
                columns: new[] { "CosmosDbId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrossConfigResourceReferences_InfraConfigId_TargetResourceId",
                table: "CrossConfigResourceReferences",
                columns: new[] { "InfraConfigId", "TargetResourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomDomains_ResourceId_EnvironmentName_DomainName",
                table: "CustomDomains",
                columns: new[] { "ResourceId", "EnvironmentName", "DomainName" },
                unique: true);

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
                name: "IX_FunctionAppEnvironmentSettings_FunctionAppId",
                table: "FunctionAppEnvironmentSettings",
                column: "FunctionAppId");

            migrationBuilder.CreateIndex(
                name: "IX_InfraConfigRepositories_InfrastructureConfigId_Alias",
                table: "InfraConfigRepositories",
                columns: new[] { "InfrastructureConfigId", "Alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InfrastructureConfigs_ProjectId",
                table: "InfrastructureConfigs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyVaultEnvironmentSettings_KeyVaultId_EnvironmentName",
                table: "KeyVaultEnvironmentSettings",
                columns: new[] { "KeyVaultId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogAnalyticsWorkspaceEnvironmentSettings_LogAnalyticsWorksp~",
                table: "LogAnalyticsWorkspaceEnvironmentSettings",
                columns: new[] { "LogAnalyticsWorkspaceId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NsgRules_NetworkSecurityGroupId_Priority_Direction",
                table: "NsgRules",
                columns: new[] { "NetworkSecurityGroupId", "Priority", "Direction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_InfraConfigId",
                table: "ParameterDefinitions",
                column: "InfraConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccessTokens_TokenHash",
                table: "PersonalAccessTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccessTokens_UserId",
                table: "PersonalAccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateEndpointConfigs_ResourceId",
                table: "PrivateEndpointConfigs",
                column: "ResourceId");

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
                name: "IX_ProjectEnvironments_ProjectId",
                table: "ProjectEnvironments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPipelineVariableGroups_ProjectId_GroupName",
                table: "ProjectPipelineVariableGroups",
                columns: new[] { "ProjectId", "GroupName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepositories_ProjectId_Alias",
                table: "ProjectRepositories",
                columns: new[] { "ProjectId", "Alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResourceAbbreviations_ProjectId_ResourceType",
                table: "ProjectResourceAbbreviations",
                columns: new[] { "ProjectId", "ResourceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResourceNamingTemplates_ProjectId_ResourceType",
                table: "ProjectResourceNamingTemplates",
                columns: new[] { "ProjectId", "ResourceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedisCacheEnvironmentSettings_RedisCacheId_EnvironmentName",
                table: "RedisCacheEnvironmentSettings",
                columns: new[] { "RedisCacheId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAbbreviationOverrides_InfraConfigId_ResourceType",
                table: "ResourceAbbreviationOverrides",
                columns: new[] { "InfraConfigId", "ResourceType" },
                unique: true);

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
                name: "IX_RoleAssignments_SourceResourceId_TargetResourceId_UserAssig~",
                table: "RoleAssignments",
                columns: new[] { "SourceResourceId", "TargetResourceId", "UserAssignedIdentityId", "RoleDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_TargetResourceId",
                table: "RoleAssignments",
                column: "TargetResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_UserAssignedIdentityId",
                table: "RoleAssignments",
                column: "UserAssignedIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_SecureParameterMappings_ResourceId_SecureParameterName",
                table: "SecureParameterMappings",
                columns: new[] { "ResourceId", "SecureParameterName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecureParameterMappings_VariableGroupId",
                table: "SecureParameterMappings",
                column: "VariableGroupId");

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

            migrationBuilder.CreateIndex(
                name: "IX_SqlDatabaseEnvironmentSettings_SqlDatabaseId",
                table: "SqlDatabaseEnvironmentSettings",
                column: "SqlDatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SqlServerEnvironmentSettings_SqlServerId",
                table: "SqlServerEnvironmentSettings",
                column: "SqlServerId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageAccountCorsRules_StorageAccountId",
                table: "StorageAccountCorsRules",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageAccountEnvironmentSettings_StorageAccountId_Environm~",
                table: "StorageAccountEnvironmentSettings",
                columns: new[] { "StorageAccountId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageQueues_StorageAccountId",
                table: "StorageQueues",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageTables_StorageAccountId",
                table: "StorageTables",
                column: "StorageAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_VirtualNetworkId",
                table: "Subnets",
                column: "VirtualNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_User_EntraId",
                table: "User",
                column: "EntraId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirtualNetworkEnvironmentSettings_VirtualNetworkId_Environm~",
                table: "VirtualNetworkEnvironmentSettings",
                columns: new[] { "VirtualNetworkId", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirtualNetworkLinks_PrivateDnsZoneId",
                table: "VirtualNetworkLinks",
                column: "PrivateDnsZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_WebAppEnvironmentSettings_WebAppId",
                table: "WebAppEnvironmentSettings",
                column: "WebAppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigurationEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "AppConfigurationKeyEnvironmentValues");

            migrationBuilder.DropTable(
                name: "ApplicationInsightsEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "AppServicePlanEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "AppSettingEnvironmentValues");

            migrationBuilder.DropTable(
                name: "AzureResourceDependencies");

            migrationBuilder.DropTable(
                name: "BlobContainers");

            migrationBuilder.DropTable(
                name: "BlobLifecycleRules");

            migrationBuilder.DropTable(
                name: "ContainerAppEnvironmentEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ContainerAppEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ContainerRegistryEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "CosmosDbEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "CrossConfigResourceReferences");

            migrationBuilder.DropTable(
                name: "CustomDomains");

            migrationBuilder.DropTable(
                name: "EventHubConsumerGroups");

            migrationBuilder.DropTable(
                name: "EventHubNamespaceEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "EventHubs");

            migrationBuilder.DropTable(
                name: "FrontDoorEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "FrontDoorOrigins");

            migrationBuilder.DropTable(
                name: "FunctionAppEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "InfraConfigRepositories");

            migrationBuilder.DropTable(
                name: "InfrastructureConfigTags");

            migrationBuilder.DropTable(
                name: "KeyVaultEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "LogAnalyticsWorkspaceEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "NsgRules");

            migrationBuilder.DropTable(
                name: "PersonalAccessTokens");

            migrationBuilder.DropTable(
                name: "PrivateEndpointConfigs");

            migrationBuilder.DropTable(
                name: "project_members");

            migrationBuilder.DropTable(
                name: "ProjectEnvironmentTags");

            migrationBuilder.DropTable(
                name: "ProjectRepositories");

            migrationBuilder.DropTable(
                name: "ProjectResourceAbbreviations");

            migrationBuilder.DropTable(
                name: "ProjectResourceNamingTemplates");

            migrationBuilder.DropTable(
                name: "ProjectTags");

            migrationBuilder.DropTable(
                name: "RedisCacheEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ResourceAbbreviationOverrides");

            migrationBuilder.DropTable(
                name: "ResourceLinks");

            migrationBuilder.DropTable(
                name: "ResourceNamingTemplates");

            migrationBuilder.DropTable(
                name: "ResourceParameterUsages");

            migrationBuilder.DropTable(
                name: "RoleAssignments");

            migrationBuilder.DropTable(
                name: "SecureParameterMappings");

            migrationBuilder.DropTable(
                name: "ServiceBusNamespaceEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "ServiceBusQueues");

            migrationBuilder.DropTable(
                name: "ServiceBusTopicSubscriptions");

            migrationBuilder.DropTable(
                name: "SqlDatabaseEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "SqlServerEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "StorageAccountCorsRules");

            migrationBuilder.DropTable(
                name: "StorageAccountEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "StorageQueues");

            migrationBuilder.DropTable(
                name: "StorageTables");

            migrationBuilder.DropTable(
                name: "Subnets");

            migrationBuilder.DropTable(
                name: "UserAssignedIdentities");

            migrationBuilder.DropTable(
                name: "VirtualNetworkEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "VirtualNetworkLinks");

            migrationBuilder.DropTable(
                name: "WebAppEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "AppConfigurationKeys");

            migrationBuilder.DropTable(
                name: "ApplicationInsights");

            migrationBuilder.DropTable(
                name: "AppServicePlans");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "ContainerAppEnvironments");

            migrationBuilder.DropTable(
                name: "ContainerApps");

            migrationBuilder.DropTable(
                name: "ContainerRegistries");

            migrationBuilder.DropTable(
                name: "CosmosDbAccounts");

            migrationBuilder.DropTable(
                name: "EventHubNamespaces");

            migrationBuilder.DropTable(
                name: "FrontDoors");

            migrationBuilder.DropTable(
                name: "FunctionApps");

            migrationBuilder.DropTable(
                name: "KeyVaults");

            migrationBuilder.DropTable(
                name: "LogAnalyticsWorkspaces");

            migrationBuilder.DropTable(
                name: "NetworkSecurityGroups");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ProjectEnvironments");

            migrationBuilder.DropTable(
                name: "RedisCaches");

            migrationBuilder.DropTable(
                name: "ParameterDefinitions");

            migrationBuilder.DropTable(
                name: "ServiceBusNamespaces");

            migrationBuilder.DropTable(
                name: "SqlDatabases");

            migrationBuilder.DropTable(
                name: "SqlServers");

            migrationBuilder.DropTable(
                name: "StorageAccounts");

            migrationBuilder.DropTable(
                name: "VirtualNetworks");

            migrationBuilder.DropTable(
                name: "PrivateDnsZones");

            migrationBuilder.DropTable(
                name: "WebApps");

            migrationBuilder.DropTable(
                name: "AppConfigurations");

            migrationBuilder.DropTable(
                name: "ProjectPipelineVariableGroups");

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
