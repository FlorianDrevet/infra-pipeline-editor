using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Maps Azure resource type names to their standard abbreviations
/// used in the <c>{resourceAbbr}</c> naming-template placeholder.
/// </summary>
public static class ResourceAbbreviationCatalog
{
    private static readonly Dictionary<string, string> Abbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.KeyVault] = "kv",
        [AzureResourceTypes.RedisCache] = "redis",
        [AzureResourceTypes.StorageAccount] = "stg",
        [AzureResourceTypes.ResourceGroup] = "rg",
        [AzureResourceTypes.AppServicePlan] = "asp",
        [AzureResourceTypes.WebApp] = "app",
        [AzureResourceTypes.FunctionApp] = "func",
        [AzureResourceTypes.UserAssignedIdentity] = "id",
        [AzureResourceTypes.AppConfiguration] = "appcs",
        [AzureResourceTypes.ContainerAppEnvironment] = "cae",
        [AzureResourceTypes.ContainerApp] = "ca",
        [AzureResourceTypes.LogAnalyticsWorkspace] = "law",
        [AzureResourceTypes.ApplicationInsights] = "appi",
        [AzureResourceTypes.CosmosDb] = "cosmos",
        [AzureResourceTypes.SqlServer] = "sql",
        [AzureResourceTypes.SqlDatabase] = "sqldb",
        [AzureResourceTypes.ServiceBusNamespace] = "sb",
        [AzureResourceTypes.ContainerRegistry] = "acr",
    };

    /// <summary>
    /// Returns the abbreviation for the given <paramref name="resourceType"/>,
    /// or the lowered type name when no abbreviation is registered.
    /// </summary>
    public static string GetAbbreviation(string resourceType)
    {
        return Abbreviations.TryGetValue(resourceType, out var abbr)
            ? abbr
            : resourceType.ToLowerInvariant();
    }

    /// <summary>
    /// Returns all registered resource type → abbreviation pairs.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetAll() => Abbreviations;
}
