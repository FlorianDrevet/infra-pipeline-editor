using System.Collections.Frozen;

namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Maps resource type names (as used by Bicep generators) to their valid Private Endpoint
/// sub-resource group IDs. Used by the Bicep generation pipeline to emit PE connections.
/// </summary>
public static class PrivateEndpointGroupIdCatalog
{
    /// <summary>Valid group IDs keyed by resource type name (matching <c>IResourceTypeBicepGenerator.ResourceTypeName</c>).</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> GroupIdsByResourceType =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["KeyVault"] = ["vault"],
            ["StorageAccount"] = ["blob", "file", "queue", "table", "dfs", "web"],
            ["AppConfiguration"] = ["configurationStores"],
            ["CosmosDb"] = ["Sql", "MongoDB", "Cassandra", "Gremlin", "Table"],
            ["SqlServer"] = ["sqlServer"],
            ["RedisCache"] = ["redisCache"],
            ["ServiceBusNamespace"] = ["namespace"],
            ["EventHubNamespace"] = ["namespace"],
            ["ContainerRegistry"] = ["registry"],
            ["WebApp"] = ["sites"],
            ["FunctionApp"] = ["sites"],
            ["ApplicationInsights"] = ["azuremonitor"],
            ["LogAnalyticsWorkspace"] = ["azuremonitor"],
            ["DocumentIntelligence"] = ["account"],
        }.ToFrozenDictionary();
}
