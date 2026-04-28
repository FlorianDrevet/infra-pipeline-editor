using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;
using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;
using InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using MediatR;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Creates MediatR commands for Azure resource types using sensible defaults.
/// Handles dependency ordering between resource types.
/// </summary>
public static class ResourceCommandFactory
{
    /// <summary>
    /// Dependencies between resource types. Key = dependent, Value = required dependency type.
    /// </summary>
    private static readonly Dictionary<string, string> ResourceDependencies = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.WebApp] = AzureResourceTypes.AppServicePlan,
        [AzureResourceTypes.FunctionApp] = AzureResourceTypes.AppServicePlan,
        [AzureResourceTypes.ContainerApp] = AzureResourceTypes.ContainerAppEnvironment,
        [AzureResourceTypes.ApplicationInsights] = AzureResourceTypes.LogAnalyticsWorkspace,
        [AzureResourceTypes.SqlDatabase] = AzureResourceTypes.SqlServer,
    };

    /// <summary>
    /// Sorts resource type/name pairs so that dependencies are created before dependents.
    /// </summary>
    public static IReadOnlyList<(string ResourceType, string Name)> OrderByDependency(
        IEnumerable<(string ResourceType, string Name)> resources)
    {
        var list = resources.ToList();
        var typeSet = new HashSet<string>(list.Select(r => r.ResourceType), StringComparer.OrdinalIgnoreCase);
        var independent = new List<(string, string)>();
        var dependent = new List<(string, string)>();

        foreach (var item in list)
        {
            if (ResourceDependencies.TryGetValue(item.ResourceType, out var dep) && typeSet.Contains(dep))
                dependent.Add(item);
            else
                independent.Add(item);
        }

        independent.AddRange(dependent);
        return independent;
    }

    /// <summary>
    /// Builds a MediatR command to create a resource with sensible defaults.
    /// Returns <c>null</c> if the resource type is not recognized.
    /// </summary>
    /// <param name="resourceType">Resource type identifier (e.g. <c>KeyVault</c>).</param>
    /// <param name="resourceGroupId">Parent resource group.</param>
    /// <param name="name">Resource display name.</param>
    /// <param name="location">Azure region.</param>
    /// <param name="createdResources">Previously created resources (type → ID) for dependency resolution.</param>
    /// <param name="extractedProperties">Optional extracted properties (from import) for overriding defaults.</param>
    public static IBaseRequest? BuildCommand(
        string resourceType,
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyDictionary<string, Guid> createdResources,
        IReadOnlyDictionary<string, object?>? extractedProperties = null,
        IReadOnlyDictionary<string, Guid>? createdResourcesByName = null,
        IReadOnlyList<string>? dependencyResourceNames = null)
    {
        return resourceType switch
        {
            AzureResourceTypes.KeyVault => new CreateKeyVaultCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.StorageAccount => new CreateStorageAccountCommand(
                resourceGroupId, name, location,
                Kind: GetStringProp(extractedProperties, "kind", "StorageV2"),
                AccessTier: GetStringProp(extractedProperties, "accessTier", "Hot"),
                AllowBlobPublicAccess: false,
                EnableHttpsTrafficOnly: true,
                MinimumTlsVersion: "TLS1_2"),

            AzureResourceTypes.AppServicePlan => new CreateAppServicePlanCommand(
                resourceGroupId, name, location,
                OsType: GetStringProp(extractedProperties, "osType", "Linux")),

            AzureResourceTypes.WebApp => BuildWebAppCommand(
                resourceGroupId, name, location, createdResources, createdResourcesByName, dependencyResourceNames, extractedProperties),

            AzureResourceTypes.FunctionApp => BuildFunctionAppCommand(
                resourceGroupId, name, location, createdResources, createdResourcesByName, dependencyResourceNames, extractedProperties),

            AzureResourceTypes.ContainerAppEnvironment => new CreateContainerAppEnvironmentCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.ContainerApp => BuildContainerAppCommand(
                resourceGroupId, name, location, createdResources, createdResourcesByName, dependencyResourceNames),

            AzureResourceTypes.RedisCache => new CreateRedisCacheCommand(
                resourceGroupId, name, location,
                RedisVersion: null,
                EnableNonSslPort: false,
                MinimumTlsVersion: "1.2",
                DisableAccessKeyAuthentication: false,
                EnableAadAuth: true),

            AzureResourceTypes.UserAssignedIdentity => new CreateUserAssignedIdentityCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.AppConfiguration => new CreateAppConfigurationCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.LogAnalyticsWorkspace => new CreateLogAnalyticsWorkspaceCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.ApplicationInsights => BuildApplicationInsightsCommand(
                resourceGroupId, name, location, createdResources, createdResourcesByName, dependencyResourceNames),

            AzureResourceTypes.CosmosDb => new CreateCosmosDbCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.SqlServer => new CreateSqlServerCommand(
                resourceGroupId, name, location,
                Version: "12.0",
                AdministratorLogin: "sqladmin"),

            AzureResourceTypes.SqlDatabase => BuildSqlDatabaseCommand(
                resourceGroupId, name, location, createdResources, createdResourcesByName, dependencyResourceNames),

            AzureResourceTypes.ServiceBusNamespace => new CreateServiceBusNamespaceCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.ContainerRegistry => new CreateContainerRegistryCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.EventHubNamespace => new CreateEventHubNamespaceCommand(
                resourceGroupId, name, location),

            _ => null,
        };
    }

    /// <summary>
    /// Checks whether a resource type requires a dependency that is not yet created.
    /// </summary>
    public static string? GetMissingDependency(
        string resourceType,
        IReadOnlyDictionary<string, Guid> createdResources,
        IReadOnlyDictionary<string, Guid>? createdResourcesByName = null,
        IReadOnlyList<string>? dependencyResourceNames = null)
    {
        if (!ResourceDependencies.TryGetValue(resourceType, out var dep))
            return null;

        if (dependencyResourceNames is { Count: > 0 })
        {
            return dependencyResourceNames.FirstOrDefault(name =>
                createdResourcesByName is null || !createdResourcesByName.ContainsKey(name));
        }

        return !createdResources.ContainsKey(dep)
            ? dep
            : null;
    }

    private static IBaseRequest? BuildWebAppCommand(
        ResourceGroupId rgId, Name name, Location location,
        IReadOnlyDictionary<string, Guid> created,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames,
        IReadOnlyDictionary<string, object?>? props)
    {
        var aspId = ResolveDependencyId(
            AzureResourceTypes.AppServicePlan,
            created,
            createdByName,
            dependencyResourceNames);

        if (aspId is null)
            return null;

        return new CreateWebAppCommand(
            rgId, name, location,
            AppServicePlanId: aspId.Value,
            RuntimeStack: GetStringProp(props, "runtimeStack", "DOTNETCORE"),
            RuntimeVersion: GetStringProp(props, "runtimeVersion", "8.0"),
            AlwaysOn: false,
            HttpsOnly: true,
            DeploymentMode: "Zip",
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
    }

    private static IBaseRequest? BuildFunctionAppCommand(
        ResourceGroupId rgId, Name name, Location location,
        IReadOnlyDictionary<string, Guid> created,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames,
        IReadOnlyDictionary<string, object?>? props)
    {
        var aspId = ResolveDependencyId(
            AzureResourceTypes.AppServicePlan,
            created,
            createdByName,
            dependencyResourceNames);

        if (aspId is null)
            return null;

        return new CreateFunctionAppCommand(
            rgId, name, location,
            AppServicePlanId: aspId.Value,
            RuntimeStack: GetStringProp(props, "runtimeStack", "DOTNET-ISOLATED"),
            RuntimeVersion: GetStringProp(props, "runtimeVersion", "8.0"),
            HttpsOnly: true,
            DeploymentMode: "Zip",
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
    }

    private static IBaseRequest? BuildContainerAppCommand(
        ResourceGroupId rgId, Name name, Location location,
        IReadOnlyDictionary<string, Guid> created,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        var caeId = ResolveDependencyId(
            AzureResourceTypes.ContainerAppEnvironment,
            created,
            createdByName,
            dependencyResourceNames);

        if (caeId is null)
            return null;

        return new CreateContainerAppCommand(
            rgId, name, location,
            ContainerAppEnvironmentId: caeId.Value,
            ContainerRegistryId: null);
    }

    private static IBaseRequest? BuildApplicationInsightsCommand(
        ResourceGroupId rgId, Name name, Location location,
        IReadOnlyDictionary<string, Guid> created,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        var lawId = ResolveDependencyId(
            AzureResourceTypes.LogAnalyticsWorkspace,
            created,
            createdByName,
            dependencyResourceNames);

        if (lawId is null)
            return null;

        return new CreateApplicationInsightsCommand(
            rgId, name, location,
            LogAnalyticsWorkspaceId: lawId.Value);
    }

    private static IBaseRequest? BuildSqlDatabaseCommand(
        ResourceGroupId rgId, Name name, Location location,
        IReadOnlyDictionary<string, Guid> created,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        var sqlServerId = ResolveDependencyId(
            AzureResourceTypes.SqlServer,
            created,
            createdByName,
            dependencyResourceNames);

        if (sqlServerId is null)
            return null;

        return new CreateSqlDatabaseCommand(
            rgId, name, location,
            SqlServerId: sqlServerId.Value,
            Collation: "SQL_Latin1_General_CP1_CI_AS");
    }

    private static Guid? ResolveDependencyId(
        string dependencyType,
        IReadOnlyDictionary<string, Guid> createdByType,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        if (dependencyResourceNames is { Count: > 0 })
        {
            if (createdByName is null)
                return null;

            foreach (var dependencyResourceName in dependencyResourceNames)
            {
                if (createdByName.TryGetValue(dependencyResourceName, out var dependencyId))
                {
                    return dependencyId;
                }
            }

            return null;
        }

        return createdByType.TryGetValue(dependencyType, out var dependencyIdByType)
            ? dependencyIdByType
            : null;
    }

    private static string GetStringProp(
        IReadOnlyDictionary<string, object?>? props, string key, string defaultValue)
    {
        if (props is null)
            return defaultValue;

        return props.TryGetValue(key, out var val) && val is string s && !string.IsNullOrWhiteSpace(s)
            ? s
            : defaultValue;
    }
}
