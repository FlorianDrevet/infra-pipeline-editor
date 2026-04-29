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

namespace InfraFlowSculptor.Application.Imports.Common;

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

            AzureResourceTypes.StorageAccount => new CreateStorageAccountCommand(
                resourceGroupId, name, location,
                Kind: GetStringProp(context.ExtractedProperties, "kind", "StorageV2"),
                AccessTier: GetStringProp(context.ExtractedProperties, "accessTier", "Hot"),
                AllowBlobPublicAccess: false,
                EnableHttpsTrafficOnly: true,
                MinimumTlsVersion: "TLS1_2"),

            AzureResourceTypes.AppServicePlan => new CreateAppServicePlanCommand(
                resourceGroupId, name, location,
                OsType: GetStringProp(context.ExtractedProperties, "osType", "Linux")),

            AzureResourceTypes.WebApp => BuildWebAppCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames, context.ExtractedProperties),

            AzureResourceTypes.FunctionApp => BuildFunctionAppCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames, context.ExtractedProperties),

            AzureResourceTypes.ContainerAppEnvironment => new CreateContainerAppEnvironmentCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.ContainerApp => BuildContainerAppCommand(
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),

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
                resourceGroupId, name, location, context.CreatedResourcesByType, context.CreatedResourcesByName, context.DependencyResourceNames),

            AzureResourceTypes.CosmosDb => new CreateCosmosDbCommand(
                resourceGroupId, name, location),

            AzureResourceTypes.SqlServer => new CreateSqlServerCommand(
                resourceGroupId, name, location,
                Version: "12.0",
                AdministratorLogin: "sqladmin"),

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
    /// Sends a dynamically-typed <see cref="IBaseRequest"/> command via MediatR with proper generic dispatch.
    /// Resolves the generic <c>IRequest&lt;TResponse&gt;</c> argument at runtime and invokes the typed overload.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Required to invoke the generic SendTypedCommandAsync<TResponse> at runtime for dynamic MediatR command dispatch.")]
    public static async Task<object?> SendCommandAsync(
        ISender mediator,
        IBaseRequest command,
        CancellationToken cancellationToken)
    {
        var requestInterface = command.GetType().GetInterfaces()
            .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRequest<>));

        if (requestInterface is null)
            return null;

        var responseType = requestInterface.GetGenericArguments()[0];
        var sendMethod = typeof(ResourceCommandFactory)
            .GetMethod(nameof(SendTypedCommandAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var genericMethod = sendMethod.MakeGenericMethod(responseType);
        var task = (Task)genericMethod.Invoke(null, [mediator, command, cancellationToken])!;
        await task.ConfigureAwait(false);

        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    /// <summary>Extracts the AzureResourceId.Value from an <c>ErrorOr&lt;T&gt;</c> result using reflection.</summary>
    public static Guid? ExtractResourceId(object? result)
    {
        if (result is null)
            return null;

        var resultType = result.GetType();
        var isErrorProp = resultType.GetProperty("IsError");
        if (isErrorProp is not null && isErrorProp.GetValue(result) is true)
            return null;

        var valueProp = resultType.GetProperty("Value");
        var value = valueProp?.GetValue(result);
        if (value is null)
            return null;

        var idProp = value.GetType().GetProperty("Id");
        var id = idProp?.GetValue(value);
        if (id is null)
            return null;

        var guidProp = id.GetType().GetProperty("Value");
        return guidProp?.GetValue(id) as Guid?;
    }

    /// <summary>Extracts error descriptions from an <c>ErrorOr&lt;T&gt;</c> result using reflection.</summary>
    public static string? ExtractErrors(object? result)
    {
        if (result is null)
            return null;

        var resultType = result.GetType();
        var errorsProp = resultType.GetProperty("Errors");
        if (errorsProp?.GetValue(result) is not IEnumerable<ErrorOr.Error> errors)
            return null;

        return string.Join("; ", errors.Select(e => e.Description));
    }

    private static async Task<TResponse> SendTypedCommandAsync<TResponse>(
        ISender mediator,
        IRequest<TResponse> command,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }

    internal static string GetStringProp(
        IReadOnlyDictionary<string, object?>? props, string key, string defaultValue)
    {
        if (props is null)
            return defaultValue;

        return props.TryGetValue(key, out var val) && val is string s && !string.IsNullOrWhiteSpace(s)
            ? s
            : defaultValue;
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

    private static IBaseRequest? BuildWebAppCommand(
        ResourceGroupId rgId, Name name, Location location,
        IReadOnlyDictionary<string, Guid> created,
        IReadOnlyDictionary<string, Guid>? createdByName,
        IReadOnlyList<string>? dependencyResourceNames,
        IReadOnlyDictionary<string, object?>? props)
    {
        var aspId = ResolveDependencyId(
            AzureResourceTypes.AppServicePlan, created, createdByName, dependencyResourceNames);

        if (aspId is null) return null;

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
            AzureResourceTypes.AppServicePlan, created, createdByName, dependencyResourceNames);

        if (aspId is null) return null;

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
    IReadOnlyDictionary<string, object?>? ExtractedProperties = null,
    IReadOnlyDictionary<string, Guid>? CreatedResourcesByName = null,
    IReadOnlyList<string>? DependencyResourceNames = null);
