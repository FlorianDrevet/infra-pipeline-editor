using System.Collections.Frozen;

namespace InfraFlowSculptor.Domain.Common.Constants;

/// <summary>Maps Azure resource types to their valid Private Endpoint sub-resource group IDs.</summary>
public static class PrivateEndpointGroupIdCatalog
{
    /// <summary>Valid group IDs keyed by resource type constant.</summary>
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
        }.ToFrozenDictionary();
}
