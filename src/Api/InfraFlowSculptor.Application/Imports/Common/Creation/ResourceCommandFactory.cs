using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Application.Imports.Common.Properties;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using MediatR;

namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Creates MediatR commands for Azure resource types using sensible defaults.
/// Handles dependency ordering between resource types.
/// Shared by both the import apply flow and the MCP project setup orchestrator.
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
    /// <param name="context">Resolution context for dependency and property lookups.</param>
    public static IBaseRequest? BuildCommand(
        string resourceType,
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        ResourceCreationContext context)
    {
        return resourceType switch
        {
            AzureResourceTypes.KeyVault => new CreateKeyVaultCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.StorageAccount => BuildStorageAccountCommand(
                resourceGroupId, name, location, context.TypedProperties),

            AzureResourceTypes.AppServicePlan => new CreateAppServicePlanCommand(
                resourceGroupId, name, location,
                OsType: context.TypedProperties is AppServicePlanExtractedProperties asp
                    ? asp.OsType
                    : AppServicePlanExtractedProperties.DefaultOsType),

            AzureResourceTypes.WebApp => BuildWebAppCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames, context.TypedProperties),

            AzureResourceTypes.FunctionApp => BuildFunctionAppCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames, context.TypedProperties),

            AzureResourceTypes.ContainerAppEnvironment => new CreateContainerAppEnvironmentCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.ContainerApp => BuildContainerAppCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),

            AzureResourceTypes.RedisCache => new CreateRedisCacheCommand(
                resourceGroupId, name, location,
                RedisVersion: null,
                EnableNonSslPort: false,
                MinimumTlsVersion: AzureResourceDefaults.MinimumTlsVersion,
                DisableAccessKeyAuthentication: false,
                EnableAadAuth: true),

            AzureResourceTypes.UserAssignedIdentity => new CreateUserAssignedIdentityCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.AppConfiguration => new CreateAppConfigurationCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.LogAnalyticsWorkspace => new CreateLogAnalyticsWorkspaceCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.ApplicationInsights => BuildApplicationInsightsCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),

            AzureResourceTypes.CosmosDb => new CreateCosmosDbCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.SqlServer => new CreateSqlServerCommand(
                resourceGroupId, name, location,
                Version: AzureResourceDefaults.SqlServerVersion,
                AdministratorLogin: AzureResourceDefaults.SqlServerAdministratorLogin),

            AzureResourceTypes.SqlDatabase => BuildSqlDatabaseCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),

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

    /// <summary>
    /// Builds, sends, and extracts the resource ID for a given resource type in a single strongly-typed dispatch.
    /// Returns the created resource's GUID on success, or an <see cref="ErrorOr"/> error.
    /// Returns <c>null</c> when the resource type is unsupported or a required dependency is missing
    /// (callers should check <see cref="GetMissingDependency"/> to distinguish the two cases).
    /// </summary>
    public static Task<ErrorOr<Guid>>? CreateResourceAsync(
        ISender mediator,
        string resourceType,
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        ResourceCreationContext context,
        CancellationToken cancellationToken)
    {
        return resourceType switch
        {
            AzureResourceTypes.KeyVault => SendAndExtractIdAsync(
                mediator,
                new CreateKeyVaultCommand(resourceGroupId, name, location),
                static (KeyVaultResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.StorageAccount => SendAndExtractIdAsync(
                mediator,
                BuildStorageAccountCommand(resourceGroupId, name, location, context.TypedProperties),
                static (StorageAccountResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.AppServicePlan => SendAndExtractIdAsync(
                mediator,
                new CreateAppServicePlanCommand(
                    resourceGroupId, name, location,
                    OsType: context.TypedProperties is AppServicePlanExtractedProperties asp
                        ? asp.OsType
                        : AppServicePlanExtractedProperties.DefaultOsType),
                static (AppServicePlanResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.WebApp => CreateDependentResourceAsync<CreateWebAppCommand, WebAppResult>(
                mediator,
                BuildWebAppCommand(resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames, context.TypedProperties),
                static (WebAppResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.FunctionApp => CreateDependentResourceAsync<CreateFunctionAppCommand, FunctionAppResult>(
                mediator,
                BuildFunctionAppCommand(resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames, context.TypedProperties),
                static (FunctionAppResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.ContainerAppEnvironment => SendAndExtractIdAsync(
                mediator,
                new CreateContainerAppEnvironmentCommand(resourceGroupId, name, location),
                static (ContainerAppEnvironmentResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.ContainerApp => CreateDependentResourceAsync<CreateContainerAppCommand, ContainerAppResult>(
                mediator,
                BuildContainerAppCommand(resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),
                static (ContainerAppResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.RedisCache => SendAndExtractIdAsync(
                mediator,
                new CreateRedisCacheCommand(
                    resourceGroupId, name, location,
                    RedisVersion: null,
                    EnableNonSslPort: false,
                    MinimumTlsVersion: AzureResourceDefaults.MinimumTlsVersion,
                    DisableAccessKeyAuthentication: false,
                    EnableAadAuth: true),
                static (RedisCacheResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.UserAssignedIdentity => SendAndExtractIdAsync(
                mediator,
                new CreateUserAssignedIdentityCommand(resourceGroupId, name, location),
                static (UserAssignedIdentityResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.AppConfiguration => SendAndExtractIdAsync(
                mediator,
                new CreateAppConfigurationCommand(resourceGroupId, name, location),
                static (AppConfigurationResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.LogAnalyticsWorkspace => SendAndExtractIdAsync(
                mediator,
                new CreateLogAnalyticsWorkspaceCommand(resourceGroupId, name, location),
                static (LogAnalyticsWorkspaceResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.ApplicationInsights => CreateDependentResourceAsync<CreateApplicationInsightsCommand, ApplicationInsightsResult>(
                mediator,
                BuildApplicationInsightsCommand(resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),
                static (ApplicationInsightsResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.CosmosDb => SendAndExtractIdAsync(
                mediator,
                new CreateCosmosDbCommand(resourceGroupId, name, location),
                static (CosmosDbResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.SqlServer => SendAndExtractIdAsync(
                mediator,
                new CreateSqlServerCommand(
                    resourceGroupId, name, location,
                    Version: AzureResourceDefaults.SqlServerVersion,
                    AdministratorLogin: AzureResourceDefaults.SqlServerAdministratorLogin),
                static (SqlServerResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.SqlDatabase => CreateDependentResourceAsync<CreateSqlDatabaseCommand, SqlDatabaseResult>(
                mediator,
                BuildSqlDatabaseCommand(resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),
                static (SqlDatabaseResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.ServiceBusNamespace => SendAndExtractIdAsync(
                mediator,
                new CreateServiceBusNamespaceCommand(resourceGroupId, name, location),
                static (ServiceBusNamespaceResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.ContainerRegistry => SendAndExtractIdAsync(
                mediator,
                new CreateContainerRegistryCommand(resourceGroupId, name, location),
                static (ContainerRegistryResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.EventHubNamespace => SendAndExtractIdAsync(
                mediator,
                new CreateEventHubNamespaceCommand(resourceGroupId, name, location),
                static (EventHubNamespaceResult r) => r.Id.Value,
                cancellationToken),

            _ => null,
        };
    }

    /// <summary>
    /// Formats a resource creation failure into a readable message.
    /// </summary>
    public static string FormatErrors(IReadOnlyList<Error> errors)
    {
        return string.Join("; ", errors.Select(e => e.Description));
    }

    private static async Task<ErrorOr<Guid>> SendAndExtractIdAsync<TCommand, TResult>(
        ISender mediator,
        TCommand command,
        Func<TResult, Guid> idSelector,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResult>
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? result.Errors
            : idSelector(result.Value);
    }

    /// <summary>
    /// Handles dependent resource types whose Build* helper returns <c>null</c> when the dependency is missing.
    /// </summary>
    private static Task<ErrorOr<Guid>>? CreateDependentResourceAsync<TCommand, TResult>(
        ISender mediator,
        IBaseRequest? command,
        Func<TResult, Guid> idSelector,
        CancellationToken cancellationToken)
        where TCommand : IBaseRequest, ICommand<TResult>
    {
        return command is TCommand typedCommand
            ? SendAndExtractIdAsync(mediator, typedCommand, idSelector, cancellationToken)
            : null;
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
                    return dependencyId;
            }

            return null;
        }

        return createdByType.TryGetValue(dependencyType, out var dependencyIdByType)
            ? dependencyIdByType
            : null;
    }

    private static CreateStorageAccountCommand BuildStorageAccountCommand(
        ResourceGroupId rgId, Name name, Location location,
        IExtractedResourceProperties? props)
    {
        var sa = props as StorageAccountExtractedProperties;
        return new CreateStorageAccountCommand(
            rgId, name, location,
            Kind: sa?.KindOrDefault ?? "StorageV2",
            AccessTier: "Hot",
            AllowBlobPublicAccess: false,
            EnableHttpsTrafficOnly: true,
            MinimumTlsVersion: "TLS1_2");
    }

    private static IBaseRequest? BuildWebAppCommand(
        ResourceGroupId rgId, Name name, Location location,
        IReadOnlyDictionary<string, Guid> created,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames,
        IExtractedResourceProperties? props)
    {
        var aspId = ResolveDependencyId(
            AzureResourceTypes.AppServicePlan, created, createdByName, dependencyResourceNames);

        if (aspId is null) return null;

        var wa = props as WebAppExtractedProperties;
        return new CreateWebAppCommand(
            rgId, name, location,
            AppServicePlanId: aspId.Value,
            RuntimeStack: wa?.RuntimeStack ?? WebAppExtractedProperties.DefaultRuntimeStack,
            RuntimeVersion: wa?.RuntimeVersion ?? WebAppExtractedProperties.DefaultRuntimeVersion,
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
        IExtractedResourceProperties? props)
    {
        var aspId = ResolveDependencyId(
            AzureResourceTypes.AppServicePlan, created, createdByName, dependencyResourceNames);

        if (aspId is null) return null;

        var fa = props as FunctionAppExtractedProperties;
        return new CreateFunctionAppCommand(
            rgId, name, location,
            AppServicePlanId: aspId.Value,
            RuntimeStack: fa?.RuntimeStack ?? FunctionAppExtractedProperties.DefaultRuntimeStack,
            RuntimeVersion: fa?.RuntimeVersion ?? FunctionAppExtractedProperties.DefaultRuntimeVersion,
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
            AzureResourceTypes.ContainerAppEnvironment, created, createdByName, dependencyResourceNames);

        if (caeId is null) return null;

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
            AzureResourceTypes.LogAnalyticsWorkspace, created, createdByName, dependencyResourceNames);

        if (lawId is null) return null;

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
            AzureResourceTypes.SqlServer, created, createdByName, dependencyResourceNames);

        if (sqlServerId is null) return null;

        return new CreateSqlDatabaseCommand(
            rgId, name, location,
            SqlServerId: sqlServerId.Value,
            Collation: "SQL_Latin1_General_CP1_CI_AS");
    }
}

/// <summary>
/// Groups dependency resolution context for resource creation commands.
/// </summary>
public sealed record ResourceCreationContext(
    IReadOnlyDictionary<string, Guid> CreatedResourcesByType,
    IExtractedResourceProperties? TypedProperties = null,
    IReadOnlyDictionary<string, Guid>? CreatedResourcesByName = null,
    IReadOnlyList<string>? DependencyResourceNames = null);
