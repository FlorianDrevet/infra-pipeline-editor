using System.Collections.Frozen;

namespace InfraFlowSculptor.Domain.Common.Constants;

/// <summary>Maps Private Endpoint group IDs to their standard Private DNS Zone names.</summary>
public static class PrivateDnsZoneNameCatalog
{
    /// <summary>DNS zone name keyed by group ID.</summary>
    public static readonly IReadOnlyDictionary<string, string> ZoneNameByGroupId =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["vault"] = "privatelink.vaultcore.azure.net",
            ["blob"] = "privatelink.blob.core.windows.net",
            ["file"] = "privatelink.file.core.windows.net",
            ["queue"] = "privatelink.queue.core.windows.net",
            ["table"] = "privatelink.table.core.windows.net",
            ["dfs"] = "privatelink.dfs.core.windows.net",
            ["web"] = "privatelink.web.core.windows.net",
            ["configurationStores"] = "privatelink.azconfig.io",
            ["Sql"] = "privatelink.documents.azure.com",
            ["sqlServer"] = "privatelink.database.windows.net",
            ["redisCache"] = "privatelink.redis.cache.windows.net",
            ["namespace"] = "privatelink.servicebus.windows.net",
            ["registry"] = "privatelink.azurecr.io",
            ["sites"] = "privatelink.azurewebsites.net",
            ["azuremonitor"] = "privatelink.monitor.azure.com",
            ["account"] = "privatelink.cognitiveservices.azure.com",
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
