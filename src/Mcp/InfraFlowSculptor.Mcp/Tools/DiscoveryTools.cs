using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides read-only discovery tools that expose static metadata about
/// the topologies and Azure resource types supported by Infra Flow Sculptor.
/// </summary>
[McpServerToolType]
public sealed class DiscoveryTools
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    // ── list_repository_topologies ──────────────────────────────────────

    /// <summary>
    /// Returns the repository layout topologies supported by Infra Flow Sculptor.
    /// </summary>
    [McpServerTool(Name = "list_repository_topologies")]
    [Description("Lists the repository layout topologies supported by Infra Flow Sculptor, with a description of each option and its implications for repository structure.")]
    public static string ListRepositoryTopologies()
    {
        var payload = new { topologies = Topologies };
        return JsonSerializer.Serialize(payload, SerializerOptions);
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
        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    // ── Static topology metadata ───────────────────────────────────────

    private static readonly object[] Topologies =
    [
        new
        {
            Id = "AllInOne",
            Label = "All-in-One (Mono Repo)",
            Description = "One single repository contains infrastructure code, application code, and all pipelines. Requires exactly 1 project-level repository.",
            RequiredRepositoryCount = 1,
            RepositoryContentKinds = new[] { new[] { "Infrastructure", "Application" } },
        },
        new
        {
            Id = "SplitInfraCode",
            Label = "Split Infra / Code",
            Description = "Two repositories: one for infrastructure (Bicep + infra pipelines), one for application code (app pipelines). Requires exactly 2 project-level repositories.",
            RequiredRepositoryCount = 2,
            RepositoryContentKinds = new[] { new[] { "Infrastructure" }, new[] { "Application" } },
        },
        new
        {
            Id = "MultiRepo",
            Label = "Multi-Repo",
            Description = "Repositories are declared per infrastructure configuration, not at project level. The project itself owns no repository. Each config can have its own layout mode.",
            RequiredRepositoryCount = 0,
            RepositoryContentKinds = Array.Empty<string[]>(),
        },
    ];

    // ── Static resource type metadata ──────────────────────────────────

    private static readonly object[] ResourceTypes =
    [
        Rt("KeyVault",                "Microsoft.KeyVault/vaults",                           "Azure Key Vault",                    "Security"),
        Rt("RedisCache",              "Microsoft.Cache/Redis",                                "Azure Cache for Redis",              "Data"),
        Rt("StorageAccount",          "Microsoft.Storage/storageAccounts",                    "Azure Storage Account",              "Storage"),
        Rt("AppServicePlan",          "Microsoft.Web/serverfarms",                            "App Service Plan",                   "Compute"),
        Rt("WebApp",                  "Microsoft.Web/sites",                                  "Web App",                            "Compute"),
        Rt("FunctionApp",             "Microsoft.Web/sites/functionapp",                      "Function App",                       "Compute"),
        Rt("ContainerAppEnvironment", "Microsoft.App/managedEnvironments",                    "Container App Environment",          "Compute"),
        Rt("ContainerApp",            "Microsoft.App/containerApps",                          "Container App",                      "Compute"),
        Rt("ContainerRegistry",       "Microsoft.ContainerRegistry/registries",               "Container Registry",                 "Compute"),
        Rt("UserAssignedIdentity",    "Microsoft.ManagedIdentity/userAssignedIdentities",     "User Assigned Managed Identity",     "Security"),
        Rt("AppConfiguration",        "Microsoft.AppConfiguration/configurationStores",       "App Configuration",                  "Configuration"),
        Rt("LogAnalyticsWorkspace",   "Microsoft.OperationalInsights/workspaces",             "Log Analytics Workspace",            "Monitoring"),
        Rt("ApplicationInsights",     "Microsoft.Insights/components",                        "Application Insights",               "Monitoring"),
        Rt("CosmosDb",                "Microsoft.DocumentDB/databaseAccounts",                "Azure Cosmos DB",                    "Data"),
        Rt("SqlServer",               "Microsoft.Sql/servers",                                "SQL Server",                         "Data"),
        Rt("SqlDatabase",             "Microsoft.Sql/servers/databases",                      "SQL Database",                       "Data"),
        Rt("ServiceBusNamespace",     "Microsoft.ServiceBus/namespaces",                      "Service Bus Namespace",              "Messaging"),
        Rt("EventHubNamespace",       "Microsoft.EventHub/namespaces",                        "Event Hubs Namespace",               "Messaging"),
    ];

    private static object Rt(string id, string armType, string label, string category) =>
        new { Id = id, ArmType = armType, Label = label, Category = category };
}
