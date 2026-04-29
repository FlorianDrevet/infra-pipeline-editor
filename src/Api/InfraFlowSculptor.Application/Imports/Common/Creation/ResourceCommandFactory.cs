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
/// Creates and dispatches MediatR commands for Azure resource types using sensible defaults.
/// Owns the resource-type → command mapping and the dependency graph used to order creations.
/// Shared by both the import apply flow and the MCP project setup orchestrator.
/// </summary>
public static class ResourceCommandFactory
{
    /// <summary>
    /// Direct dependencies between resource types. Key = dependent type, Value = required dependency type.
    /// Single source of truth used by both topological ordering and missing-dependency detection.
    /// </summary>
    private static readonly Dictionary<string, string> ResourceDependencies = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.WebApp] = AzureResourceTypes.AppServicePlan,
        [AzureResourceTypes.FunctionApp] = AzureResourceTypes.AppServicePlan,
        [AzureResourceTypes.ContainerApp] = AzureResourceTypes.ContainerAppEnvironment,
        [AzureResourceTypes.ApplicationInsights] = AzureResourceTypes.LogAnalyticsWorkspace,
        [AzureResourceTypes.SqlDatabase] = AzureResourceTypes.SqlServer,
    };

    private static readonly HashSet<string> SupportedResourceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        AzureResourceTypes.KeyVault,
        AzureResourceTypes.StorageAccount,
        AzureResourceTypes.AppServicePlan,
        AzureResourceTypes.WebApp,
        AzureResourceTypes.FunctionApp,
        AzureResourceTypes.ContainerAppEnvironment,
        AzureResourceTypes.ContainerApp,
        AzureResourceTypes.RedisCache,
        AzureResourceTypes.UserAssignedIdentity,
        AzureResourceTypes.AppConfiguration,
        AzureResourceTypes.LogAnalyticsWorkspace,
        AzureResourceTypes.ApplicationInsights,
        AzureResourceTypes.CosmosDb,
        AzureResourceTypes.SqlServer,
        AzureResourceTypes.SqlDatabase,
        AzureResourceTypes.ServiceBusNamespace,
        AzureResourceTypes.ContainerRegistry,
        AzureResourceTypes.EventHubNamespace,
    };

    /// <summary>
    /// Returns whether the given resource type identifier is dispatchable by <see cref="CreateResourceAsync"/>.
    /// </summary>
    public static bool IsSupported(string resourceType) => SupportedResourceTypes.Contains(resourceType);

    /// <summary>
    /// Sorts resource type/name pairs so that direct and transitive dependencies are created before their dependents.
    /// Implementation: Kahn's topological sort. Resources whose declared dependency is not present in the input
    /// list are treated as roots (no edge added) so the import flow can decide later whether to fail.
    /// Stable: input order is preserved for nodes with the same in-degree.
    /// </summary>
    public static IReadOnlyList<(string ResourceType, string Name)> OrderByDependency(
        IEnumerable<(string ResourceType, string Name)> resources)
    {
        var nodes = resources.ToList();
        if (nodes.Count <= 1)
            return nodes;

        var typesPresent = new HashSet<string>(
            nodes.Select(n => n.ResourceType),
            StringComparer.OrdinalIgnoreCase);

        var inDegree = new int[nodes.Count];
        var adjacency = new List<List<int>>(nodes.Count);
        for (var i = 0; i < nodes.Count; i++)
            adjacency.Add([]);

        for (var dependentIndex = 0; dependentIndex < nodes.Count; dependentIndex++)
        {
            if (!ResourceDependencies.TryGetValue(nodes[dependentIndex].ResourceType, out var requiredType))
                continue;

            if (!typesPresent.Contains(requiredType))
                continue;

            for (var sourceIndex = 0; sourceIndex < nodes.Count; sourceIndex++)
            {
                if (sourceIndex == dependentIndex)
                    continue;

                if (!string.Equals(nodes[sourceIndex].ResourceType, requiredType, StringComparison.OrdinalIgnoreCase))
                    continue;

                adjacency[sourceIndex].Add(dependentIndex);
                inDegree[dependentIndex]++;
            }
        }

        var ordered = new List<(string, string)>(nodes.Count);
        var ready = new Queue<int>();
        for (var i = 0; i < nodes.Count; i++)
        {
            if (inDegree[i] == 0)
                ready.Enqueue(i);
        }

        while (ready.Count > 0)
        {
            var index = ready.Dequeue();
            ordered.Add(nodes[index]);

            foreach (var next in adjacency[index])
            {
                if (--inDegree[next] == 0)
                    ready.Enqueue(next);
            }
        }

        // Defensive cycle fallback: keep remaining nodes so callers never lose data.
        if (ordered.Count != nodes.Count)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (inDegree[i] > 0)
                    ordered.Add(nodes[i]);
            }
        }

        return ordered;
    }

    /// <summary>
    /// Checks whether a resource type requires a dependency that is not yet created.
    /// Returns the missing dependency name (or type) if any, otherwise <c>null</c>.
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
    /// Returns the created resource's GUID on success, or an <see cref="ErrorOr"/> error on failure.
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
                BuildWebAppCommand(resourceGroupId, name, location, context),
                static (WebAppResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.FunctionApp => CreateDependentResourceAsync<CreateFunctionAppCommand, FunctionAppResult>(
                mediator,
                BuildFunctionAppCommand(resourceGroupId, name, location, context),
                static (FunctionAppResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.ContainerAppEnvironment => SendAndExtractIdAsync(
                mediator,
                new CreateContainerAppEnvironmentCommand(resourceGroupId, name, location),
                static (ContainerAppEnvironmentResult r) => r.Id.Value,
                cancellationToken),

            AzureResourceTypes.ContainerApp => CreateDependentResourceAsync<CreateContainerAppCommand, ContainerAppResult>(
                mediator,
                BuildContainerAppCommand(resourceGroupId, name, location, context),
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
                BuildApplicationInsightsCommand(resourceGroupId, name, location, context),
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
                BuildSqlDatabaseCommand(resourceGroupId, name, location, context),
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

    private static Guid? ResolveDependencyId(string dependencyType, ResourceCreationContext context)
    {
        if (context.DependencyResourceNames is { Count: > 0 })
        {
            if (context.CreatedResourcesByName is null)
                return null;

            foreach (var dependencyResourceName in context.DependencyResourceNames)
            {
                if (context.CreatedResourcesByName.TryGetValue(dependencyResourceName, out var dependencyId))
                    return dependencyId;
            }

            return null;
        }

        return context.CreatedResourcesByType.TryGetValue(dependencyType, out var dependencyIdByType)
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
            Kind: sa?.KindOrDefault ?? AzureResourceDefaults.StorageAccountKind,
            AccessTier: AzureResourceDefaults.StorageAccountAccessTier,
            AllowBlobPublicAccess: false,
            EnableHttpsTrafficOnly: true,
            MinimumTlsVersion: AzureResourceDefaults.MinimumTlsVersionLabel);
    }

    private static IBaseRequest? BuildWebAppCommand(
        ResourceGroupId rgId, Name name, Location location, ResourceCreationContext context)
    {
        var aspId = ResolveDependencyId(AzureResourceTypes.AppServicePlan, context);
        if (aspId is null) return null;

        var wa = context.TypedProperties as WebAppExtractedProperties;
        return new CreateWebAppCommand(
            rgId, name, location,
            AppServicePlanId: aspId.Value,
            RuntimeStack: wa?.RuntimeStack ?? WebAppExtractedProperties.DefaultRuntimeStack,
            RuntimeVersion: wa?.RuntimeVersion ?? WebAppExtractedProperties.DefaultRuntimeVersion,
            AlwaysOn: false,
            HttpsOnly: true,
            DeploymentMode: AzureResourceDefaults.AppServiceDeploymentMode,
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
    }

    private static IBaseRequest? BuildFunctionAppCommand(
        ResourceGroupId rgId, Name name, Location location, ResourceCreationContext context)
    {
        var aspId = ResolveDependencyId(AzureResourceTypes.AppServicePlan, context);
        if (aspId is null) return null;

        var fa = context.TypedProperties as FunctionAppExtractedProperties;
        return new CreateFunctionAppCommand(
            rgId, name, location,
            AppServicePlanId: aspId.Value,
            RuntimeStack: fa?.RuntimeStack ?? FunctionAppExtractedProperties.DefaultRuntimeStack,
            RuntimeVersion: fa?.RuntimeVersion ?? FunctionAppExtractedProperties.DefaultRuntimeVersion,
            HttpsOnly: true,
            DeploymentMode: AzureResourceDefaults.AppServiceDeploymentMode,
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
    }

    private static IBaseRequest? BuildContainerAppCommand(
        ResourceGroupId rgId, Name name, Location location, ResourceCreationContext context)
    {
        var caeId = ResolveDependencyId(AzureResourceTypes.ContainerAppEnvironment, context);
        if (caeId is null) return null;

        return new CreateContainerAppCommand(
            rgId, name, location,
            ContainerAppEnvironmentId: caeId.Value,
            ContainerRegistryId: null);
    }

    private static IBaseRequest? BuildApplicationInsightsCommand(
        ResourceGroupId rgId, Name name, Location location, ResourceCreationContext context)
    {
        var lawId = ResolveDependencyId(AzureResourceTypes.LogAnalyticsWorkspace, context);
        if (lawId is null) return null;

        return new CreateApplicationInsightsCommand(
            rgId, name, location,
            LogAnalyticsWorkspaceId: lawId.Value);
    }

    private static IBaseRequest? BuildSqlDatabaseCommand(
        ResourceGroupId rgId, Name name, Location location, ResourceCreationContext context)
    {
        var sqlServerId = ResolveDependencyId(AzureResourceTypes.SqlServer, context);
        if (sqlServerId is null) return null;

        return new CreateSqlDatabaseCommand(
            rgId, name, location,
            SqlServerId: sqlServerId.Value,
            Collation: AzureResourceDefaults.SqlDatabaseCollation);
    }
}
