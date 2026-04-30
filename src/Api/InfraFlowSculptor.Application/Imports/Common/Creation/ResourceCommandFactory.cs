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
    /// Default values used when auto-creating resources. These must match the domain enum names
    /// (e.g. <c>Tls12</c>, <c>DotNet</c>, <c>Code</c>) because Application command handlers parse
    /// them via <see cref="Enum.Parse(Type, string)"/>.
    /// </summary>
    private const string DefaultTlsVersionEnumName = "Tls12";
    private const string DefaultDotNetRuntimeStackEnumName = "DotNet";
    private const string DefaultWebRuntimeStackEnumName = DefaultDotNetRuntimeStackEnumName;
    private const string DefaultFunctionRuntimeStackEnumName = DefaultDotNetRuntimeStackEnumName;
    private const string DefaultDeploymentModeEnumName = "Code";
    private const string DefaultSqlServerVersionEnumName = "V12";
    private const string DefaultWebRuntimeVersion = "8";
    private const string DefaultFunctionRuntimeVersion = "8-isolated";

    private delegate Task<ErrorOr<Guid>>? ResourceCreationDispatcher(
        ISender mediator,
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        ResourceCreationContext context,
        CancellationToken cancellationToken);

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

    private static readonly Dictionary<string, ResourceCreationDispatcher> ResourceCreationDispatchers = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.KeyVault] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateKeyVaultCommand(resourceGroupId, name, location),
                static (KeyVaultResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.StorageAccount] = static (mediator, resourceGroupId, name, location, context, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                BuildStorageAccountCommand(resourceGroupId, name, location, context.TypedProperties),
                static (StorageAccountResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.AppServicePlan] = static (mediator, resourceGroupId, name, location, context, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateAppServicePlanCommand(
                    resourceGroupId,
                    name,
                    location,
                    OsType: context.TypedProperties is AppServicePlanExtractedProperties appServicePlan
                        ? appServicePlan.OsType
                        : AppServicePlanExtractedProperties.DefaultOsType),
                static (AppServicePlanResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.WebApp] = static (mediator, resourceGroupId, name, location, context, cancellationToken) =>
            CreateDependentResourceAsync<CreateWebAppCommand, WebAppResult>(
                mediator,
                BuildWebAppCommand(resourceGroupId, name, location, context),
                static (WebAppResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.FunctionApp] = static (mediator, resourceGroupId, name, location, context, cancellationToken) =>
            CreateDependentResourceAsync<CreateFunctionAppCommand, FunctionAppResult>(
                mediator,
                BuildFunctionAppCommand(resourceGroupId, name, location, context),
                static (FunctionAppResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.ContainerAppEnvironment] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateContainerAppEnvironmentCommand(resourceGroupId, name, location),
                static (ContainerAppEnvironmentResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.ContainerApp] = static (mediator, resourceGroupId, name, location, context, cancellationToken) =>
            CreateDependentResourceAsync<CreateContainerAppCommand, ContainerAppResult>(
                mediator,
                BuildContainerAppCommand(resourceGroupId, name, location, context),
                static (ContainerAppResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.RedisCache] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateRedisCacheCommand(
                    resourceGroupId,
                    name,
                    location,
                    RedisVersion: null,
                    EnableNonSslPort: false,
                    MinimumTlsVersion: DefaultTlsVersionEnumName,
                    DisableAccessKeyAuthentication: false,
                    EnableAadAuth: true),
                static (RedisCacheResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.UserAssignedIdentity] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateUserAssignedIdentityCommand(resourceGroupId, name, location),
                static (UserAssignedIdentityResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.AppConfiguration] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateAppConfigurationCommand(resourceGroupId, name, location),
                static (AppConfigurationResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.LogAnalyticsWorkspace] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateLogAnalyticsWorkspaceCommand(resourceGroupId, name, location),
                static (LogAnalyticsWorkspaceResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.ApplicationInsights] = static (mediator, resourceGroupId, name, location, context, cancellationToken) =>
            CreateDependentResourceAsync<CreateApplicationInsightsCommand, ApplicationInsightsResult>(
                mediator,
                BuildApplicationInsightsCommand(resourceGroupId, name, location, context),
                static (ApplicationInsightsResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.CosmosDb] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateCosmosDbCommand(resourceGroupId, name, location),
                static (CosmosDbResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.SqlServer] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateSqlServerCommand(
                    resourceGroupId,
                    name,
                    location,
                    Version: DefaultSqlServerVersionEnumName,
                    AdministratorLogin: AzureResourceDefaults.SqlServerAdministratorLogin),
                static (SqlServerResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.SqlDatabase] = static (mediator, resourceGroupId, name, location, context, cancellationToken) =>
            CreateDependentResourceAsync<CreateSqlDatabaseCommand, SqlDatabaseResult>(
                mediator,
                BuildSqlDatabaseCommand(resourceGroupId, name, location, context),
                static (SqlDatabaseResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.ServiceBusNamespace] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateServiceBusNamespaceCommand(resourceGroupId, name, location),
                static (ServiceBusNamespaceResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.ContainerRegistry] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateContainerRegistryCommand(resourceGroupId, name, location),
                static (ContainerRegistryResult result) => result.Id.Value,
                cancellationToken),

        [AzureResourceTypes.EventHubNamespace] = static (mediator, resourceGroupId, name, location, _, cancellationToken) =>
            SendAndExtractIdAsync(
                mediator,
                new CreateEventHubNamespaceCommand(resourceGroupId, name, location),
                static (EventHubNamespaceResult result) => result.Id.Value,
                cancellationToken),
    };

    /// <summary>
    /// Returns the required dependency type for a given resource type, or <c>null</c> if it has no dependency.
    /// </summary>
    public static string? GetRequiredDependencyType(string resourceType)
    {
        return ResourceDependencies.TryGetValue(resourceType, out var dep) ? dep : null;
    }

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

        var (inDegree, adjacency) = BuildDependencyGraph(nodes);
        var ordered = TopologicallySortNodes(nodes, adjacency, inDegree);
        AppendRemainingNodes(nodes, inDegree, ordered);

        return ordered;
    }

    private static (int[] InDegree, List<List<int>> Adjacency) BuildDependencyGraph(
        IReadOnlyList<(string ResourceType, string Name)> nodes)
    {
        var typesPresent = new HashSet<string>(
            nodes.Select(node => node.ResourceType),
            StringComparer.OrdinalIgnoreCase);
        var inDegree = new int[nodes.Count];
        var adjacency = CreateAdjacency(nodes.Count);

        for (var dependentIndex = 0; dependentIndex < nodes.Count; dependentIndex++)
        {
            var requiredType = GetPresentRequiredDependencyType(nodes[dependentIndex].ResourceType, typesPresent);
            if (requiredType is null)
                continue;

            AddDependencyEdges(nodes, adjacency, inDegree, dependentIndex, requiredType);
        }

        return (inDegree, adjacency);
    }

    private static List<List<int>> CreateAdjacency(int nodeCount)
    {
        var adjacency = new List<List<int>>(nodeCount);
        for (var index = 0; index < nodeCount; index++)
            adjacency.Add([]);

        return adjacency;
    }

    private static string? GetPresentRequiredDependencyType(
        string resourceType,
        IReadOnlySet<string> typesPresent)
    {
        return ResourceDependencies.TryGetValue(resourceType, out var requiredType)
               && typesPresent.Contains(requiredType)
            ? requiredType
            : null;
    }

    private static void AddDependencyEdges(
        IReadOnlyList<(string ResourceType, string Name)> nodes,
        IReadOnlyList<List<int>> adjacency,
        int[] inDegree,
        int dependentIndex,
        string requiredType)
    {
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

    private static List<(string ResourceType, string Name)> TopologicallySortNodes(
        IReadOnlyList<(string ResourceType, string Name)> nodes,
        IReadOnlyList<List<int>> adjacency,
        int[] inDegree)
    {
        var ordered = new List<(string ResourceType, string Name)>(nodes.Count);
        var ready = CreateReadyQueue(inDegree);

        while (ready.Count > 0)
        {
            var index = ready.Dequeue();
            ordered.Add(nodes[index]);
            ReleaseReadyDependents(adjacency[index], inDegree, ready);
        }

        return ordered;
    }

    private static Queue<int> CreateReadyQueue(IReadOnlyList<int> inDegree)
    {
        var ready = new Queue<int>();
        for (var index = 0; index < inDegree.Count; index++)
        {
            if (inDegree[index] == 0)
                ready.Enqueue(index);
        }

        return ready;
    }

    private static void ReleaseReadyDependents(
        IReadOnlyList<int> dependentIndexes,
        int[] inDegree,
        Queue<int> ready)
    {
        foreach (var dependentIndex in dependentIndexes)
        {
            if (--inDegree[dependentIndex] == 0)
                ready.Enqueue(dependentIndex);
        }
    }

    private static void AppendRemainingNodes(
        IReadOnlyList<(string ResourceType, string Name)> nodes,
        IReadOnlyList<int> inDegree,
        ICollection<(string ResourceType, string Name)> ordered)
    {
        // Defensive cycle fallback: keep remaining nodes so callers never lose data.
        if (ordered.Count == nodes.Count)
            return;

        for (var index = 0; index < nodes.Count; index++)
        {
            if (inDegree[index] > 0)
                ordered.Add(nodes[index]);
        }
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
        return ResourceCreationDispatchers.TryGetValue(resourceType, out var dispatcher)
            ? dispatcher(mediator, resourceGroupId, name, location, context, cancellationToken)
            : null;
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
            MinimumTlsVersion: DefaultTlsVersionEnumName);
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
            RuntimeStack: NormalizeWebRuntimeStack(wa?.RuntimeStack),
            RuntimeVersion: NormalizeWebRuntimeVersion(wa?.RuntimeStack, wa?.RuntimeVersion),
            AlwaysOn: false,
            HttpsOnly: true,
            DeploymentMode: DefaultDeploymentModeEnumName,
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
            RuntimeStack: NormalizeFunctionRuntimeStack(fa?.RuntimeStack),
            RuntimeVersion: NormalizeFunctionRuntimeVersion(fa?.RuntimeStack, fa?.RuntimeVersion),
            HttpsOnly: true,
            DeploymentMode: DefaultDeploymentModeEnumName,
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

    /// <summary>
    /// Maps a runtime-stack string (which may be either an ARM/Azure label like <c>"DOTNETCORE"</c>
    /// or already an enum name like <c>"DotNet"</c>) to the matching <c>WebAppRuntimeStackEnum</c> name.
    /// Falls back to <see cref="DefaultWebRuntimeStackEnumName"/> when the input cannot be resolved.
    /// </summary>
    private static string NormalizeWebRuntimeStack(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DefaultWebRuntimeStackEnumName;

        if (Enum.TryParse<Domain.WebAppAggregate.ValueObjects.WebAppRuntimeStack.WebAppRuntimeStackEnum>(value, ignoreCase: true, out var direct))
            return direct.ToString();

        var armToken = value.Trim().ToUpperInvariant();
        return armToken switch
        {
            "DOTNETCORE" or "DOTNET" or "DOTNET-ISOLATED" => DefaultDotNetRuntimeStackEnumName,
            "NODE" => "Node",
            "PYTHON" => "Python",
            "JAVA" => "Java",
            "PHP" => "Php",
            _ => DefaultWebRuntimeStackEnumName,
        };
    }

    /// <summary>
    /// Maps a runtime-stack string for Function Apps to the matching <c>FunctionAppRuntimeStackEnum</c> name.
    /// </summary>
    private static string NormalizeFunctionRuntimeStack(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DefaultFunctionRuntimeStackEnumName;

        if (Enum.TryParse<Domain.FunctionAppAggregate.ValueObjects.FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(value, ignoreCase: true, out var direct))
            return direct.ToString();

        var armToken = value.Trim().ToUpperInvariant();
        return armToken switch
        {
            "DOTNET-ISOLATED" or "DOTNET" or "DOTNETCORE" => DefaultDotNetRuntimeStackEnumName,
            "NODE" => "Node",
            "PYTHON" => "Python",
            "JAVA" => "Java",
            "POWERSHELL" => "PowerShell",
            _ => DefaultFunctionRuntimeStackEnumName,
        };
    }

    /// <summary>
    /// Returns a runtime version that is valid for the resolved Web App stack per <see cref="Domain.Common.Catalogs.RuntimeVersionCatalog"/>.
    /// Falls back to the most recent supported version when the input value is missing or unrecognized.
    /// </summary>
    private static string NormalizeWebRuntimeVersion(string? stackInput, string? versionInput)
    {
        var stackName = NormalizeWebRuntimeStack(stackInput);
        var stack = Enum.Parse<Domain.WebAppAggregate.ValueObjects.WebAppRuntimeStack.WebAppRuntimeStackEnum>(stackName);
        var supported = Domain.Common.Catalogs.RuntimeVersionCatalog.GetWebAppVersions(stack);
        if (!string.IsNullOrWhiteSpace(versionInput) && supported.Contains(versionInput))
            return versionInput;
        return supported.Count > 0 ? supported[0] : DefaultWebRuntimeVersion;
    }

    /// <summary>
    /// Returns a runtime version that is valid for the resolved Function App stack per <see cref="Domain.Common.Catalogs.RuntimeVersionCatalog"/>.
    /// Falls back to the most recent supported version when the input value is missing or unrecognized.
    /// </summary>
    private static string NormalizeFunctionRuntimeVersion(string? stackInput, string? versionInput)
    {
        var stackName = NormalizeFunctionRuntimeStack(stackInput);
        var stack = Enum.Parse<Domain.FunctionAppAggregate.ValueObjects.FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(stackName);
        var supported = Domain.Common.Catalogs.RuntimeVersionCatalog.GetFunctionAppVersions(stack);
        if (!string.IsNullOrWhiteSpace(versionInput) && supported.Contains(versionInput))
            return versionInput;
        return supported.Count > 0 ? supported[0] : DefaultFunctionRuntimeVersion;
    }
}
