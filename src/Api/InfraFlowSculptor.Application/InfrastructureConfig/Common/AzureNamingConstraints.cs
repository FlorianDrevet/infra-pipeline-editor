using System.Text.RegularExpressions;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Azure naming constraint for a specific resource type.
/// Used to validate that naming template static characters comply with Azure rules.
/// </summary>
public sealed record AzureNamingConstraint(
    string ResourceType,
    Regex InvalidStaticCharsRegex,
    string AllowedCharsDescription,
    int MinLength,
    int MaxLength,
    string? RecommendedTemplate = null);

/// <summary>
/// Catalog of Azure naming constraints per resource type.
/// </summary>
public static class AzureNamingConstraints
{
    private static readonly Dictionary<string, AzureNamingConstraint> Constraints = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.ContainerRegistry] = new(
            AzureResourceTypes.ContainerRegistry,
            new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters only (no hyphens or underscores)",
            5, 50,
            RecommendedTemplate: "{name}{resourceAbbr}{envShort}"),

        [AzureResourceTypes.StorageAccount] = new(
            AzureResourceTypes.StorageAccount,
            new Regex("[^a-z0-9]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "lowercase alphanumeric characters only",
            3, 24,
            RecommendedTemplate: "{name}{resourceAbbr}{envShort}"),

        [AzureResourceTypes.KeyVault] = new(
            AzureResourceTypes.KeyVault,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            3, 24),

        [AzureResourceTypes.SqlServer] = new(
            AzureResourceTypes.SqlServer,
            new Regex("[^a-z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "lowercase alphanumeric characters and hyphens",
            1, 63),

        [AzureResourceTypes.SqlDatabase] = new(
            AzureResourceTypes.SqlDatabase,
            new Regex("[^a-zA-Z0-9\\-_.]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters, hyphens, underscores, and periods",
            1, 128),

        [AzureResourceTypes.CosmosDb] = new(
            AzureResourceTypes.CosmosDb,
            new Regex("[^a-z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "lowercase alphanumeric characters and hyphens",
            3, 44),

        [AzureResourceTypes.RedisCache] = new(
            AzureResourceTypes.RedisCache,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            1, 63),

        [AzureResourceTypes.ServiceBusNamespace] = new(
            AzureResourceTypes.ServiceBusNamespace,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            6, 50),

        [AzureResourceTypes.EventHubNamespace] = new(
            AzureResourceTypes.EventHubNamespace,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            6, 50),

        [AzureResourceTypes.LogAnalyticsWorkspace] = new(
            AzureResourceTypes.LogAnalyticsWorkspace,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            4, 63),

        [AzureResourceTypes.ApplicationInsights] = new(
            AzureResourceTypes.ApplicationInsights,
            new Regex("[^a-zA-Z0-9\\-_.()]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters, hyphens, underscores, periods, and parentheses",
            1, 260),

        [AzureResourceTypes.AppServicePlan] = new(
            AzureResourceTypes.AppServicePlan,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            1, 40),

        [AzureResourceTypes.WebApp] = new(
            AzureResourceTypes.WebApp,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            2, 60),

        [AzureResourceTypes.FunctionApp] = new(
            AzureResourceTypes.FunctionApp,
            new Regex("[^a-zA-Z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters and hyphens",
            2, 60),

        [AzureResourceTypes.UserAssignedIdentity] = new(
            AzureResourceTypes.UserAssignedIdentity,
            new Regex("[^a-zA-Z0-9\\-_]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters, hyphens, and underscores",
            3, 128),

        [AzureResourceTypes.AppConfiguration] = new(
            AzureResourceTypes.AppConfiguration,
            new Regex("[^a-zA-Z0-9\\-_]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters, hyphens, and underscores",
            5, 50),

        [AzureResourceTypes.ContainerAppEnvironment] = new(
            AzureResourceTypes.ContainerAppEnvironment,
            new Regex("[^a-z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "lowercase alphanumeric characters and hyphens",
            2, 32),

        [AzureResourceTypes.ContainerApp] = new(
            AzureResourceTypes.ContainerApp,
            new Regex("[^a-z0-9\\-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "lowercase alphanumeric characters and hyphens",
            2, 32),

        [AzureResourceTypes.ResourceGroup] = new(
            AzureResourceTypes.ResourceGroup,
            new Regex("[^a-zA-Z0-9\\-_.()]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250)),
            "alphanumeric characters, hyphens, underscores, periods, and parentheses",
            1, 90),
    };

    /// <summary>
    /// Returns the naming constraint for the given resource type, or <c>null</c> if no constraint is registered.
    /// </summary>
    public static AzureNamingConstraint? GetConstraint(string resourceType)
    {
        return Constraints.GetValueOrDefault(resourceType);
    }

    /// <summary>
    /// Returns all registered constraints.
    /// </summary>
    public static IReadOnlyDictionary<string, AzureNamingConstraint> GetAll() => Constraints;
}
