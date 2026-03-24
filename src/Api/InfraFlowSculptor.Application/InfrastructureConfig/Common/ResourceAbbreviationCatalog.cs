namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Maps Azure resource type names to their standard abbreviations
/// used in the <c>{resourceAbbr}</c> naming-template placeholder.
/// </summary>
public static class ResourceAbbreviationCatalog
{
    private static readonly Dictionary<string, string> Abbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KeyVault"] = "kv",
        ["RedisCache"] = "redis",
        ["StorageAccount"] = "stg",
        ["ResourceGroup"] = "rg",
        ["AppServicePlan"] = "asp",
        ["WebApp"] = "app",
        ["FunctionApp"] = "func",
        ["UserAssignedIdentity"] = "id",
        ["AppConfiguration"] = "appcs",
        ["ContainerAppEnvironment"] = "cae",
        ["ContainerApp"] = "ca",
        ["LogAnalyticsWorkspace"] = "law",
        ["ApplicationInsights"] = "appi",
        ["CosmosDb"] = "cosmos",
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
