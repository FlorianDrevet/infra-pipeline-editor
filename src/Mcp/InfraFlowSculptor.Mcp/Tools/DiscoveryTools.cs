using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides read-only discovery tools that expose static metadata about
/// the topologies and Azure resource types supported by Infra Flow Sculptor.
/// </summary>
[McpServerToolType]
public sealed class DiscoveryTools
{

    // ── list_repository_topologies ──────────────────────────────────────

    /// <summary>
    /// Returns the repository layout topologies supported by Infra Flow Sculptor.
    /// </summary>
    [McpServerTool(Name = "list_repository_topologies")]
    [Description("Lists the repository layout topologies supported by Infra Flow Sculptor, with a description of each option and its implications for repository structure.")]
    public static string ListRepositoryTopologies()
    {
        var payload = new { topologies = Topologies };
        return JsonSerializer.Serialize(payload, McpJsonDefaults.SerializerOptions);
    }

    // ── list_supported_resource_types ───────────────────────────────────

    /// <summary>
    /// Returns all Azure resource types supported by Infra Flow Sculptor.
    /// </summary>
    [McpServerTool(Name = "list_supported_resource_types")]
    [Description("Lists all Azure resource types supported by Infra Flow Sculptor, including their identifiers and ARM provider types.")]
    public static string ListSupportedResourceTypes()
    {
        var payload = new { resourceTypes = ResourceTypes };
        return JsonSerializer.Serialize(payload, McpJsonDefaults.SerializerOptions);
    }

    // ── Static topology metadata ───────────────────────────────────────

    private static readonly TopologyInfo[] Topologies =
    [
        new(
            Id: nameof(LayoutPresetEnum.AllInOne),
            Label: "All-in-One (Mono Repo)",
            Description: "One single repository contains infrastructure code, application code, and all pipelines. Requires exactly 1 project-level repository.",
            RequiredRepositoryCount: 1,
            RepositoryContentKinds: [["Infrastructure", "Application"]]),
        new(
            Id: nameof(LayoutPresetEnum.SplitInfraCode),
            Label: "Split Infra / Code",
            Description: "Two repositories: one for infrastructure (Bicep + infra pipelines), one for application code (app pipelines). Requires exactly 2 project-level repositories.",
            RequiredRepositoryCount: 2,
            RepositoryContentKinds: [["Infrastructure"], ["Application"]]),
        new(
            Id: nameof(LayoutPresetEnum.MultiRepo),
            Label: "Multi-Repo",
            Description: "Repositories are declared per infrastructure configuration, not at project level. The project itself owns no repository. Each config can have its own layout mode.",
            RequiredRepositoryCount: 0,
            RepositoryContentKinds: []),
    ];

    // ── Static resource type metadata ──────────────────────────────────

    private static readonly ResourceTypeInfo[] ResourceTypes =
    [
        new("KeyVault",                "Microsoft.KeyVault/vaults",                           "Azure Key Vault",                    "Security"),
        new("RedisCache",              "Microsoft.Cache/Redis",                                "Azure Cache for Redis",              "Data"),
        new("StorageAccount",          "Microsoft.Storage/storageAccounts",                    "Azure Storage Account",              "Storage"),
        new("AppServicePlan",          "Microsoft.Web/serverfarms",                            "App Service Plan",                   "Compute"),
        new("WebApp",                  "Microsoft.Web/sites",                                  "Web App",                            "Compute"),
        new("FunctionApp",             "Microsoft.Web/sites/functionapp",                      "Function App",                       "Compute"),
        new("ContainerAppEnvironment", "Microsoft.App/managedEnvironments",                    "Container App Environment",          "Compute"),
        new("ContainerApp",            "Microsoft.App/containerApps",                          "Container App",                      "Compute"),
        new("ContainerRegistry",       "Microsoft.ContainerRegistry/registries",               "Container Registry",                 "Compute"),
        new("UserAssignedIdentity",    "Microsoft.ManagedIdentity/userAssignedIdentities",     "User Assigned Managed Identity",     "Security"),
        new("AppConfiguration",        "Microsoft.AppConfiguration/configurationStores",       "App Configuration",                  "Configuration"),
        new("LogAnalyticsWorkspace",   "Microsoft.OperationalInsights/workspaces",             "Log Analytics Workspace",            "Monitoring"),
        new("ApplicationInsights",     "Microsoft.Insights/components",                        "Application Insights",               "Monitoring"),
        new("CosmosDb",                "Microsoft.DocumentDB/databaseAccounts",                "Azure Cosmos DB",                    "Data"),
        new("SqlServer",               "Microsoft.Sql/servers",                                "SQL Server",                         "Data"),
        new("SqlDatabase",             "Microsoft.Sql/servers/databases",                      "SQL Database",                       "Data"),
        new("ServiceBusNamespace",     "Microsoft.ServiceBus/namespaces",                      "Service Bus Namespace",              "Messaging"),
        new("EventHubNamespace",       "Microsoft.EventHub/namespaces",                        "Event Hubs Namespace",               "Messaging"),
    ];
}
