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
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using MediatR;
using System.Reflection;

namespace InfraFlowSculptor.Application.Imports.Common;

internal static class ImportResourceCreationDispatcher
{
    private static readonly Dictionary<string, string> ResourceDependencies = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.WebApp] = AzureResourceTypes.AppServicePlan,
        [AzureResourceTypes.FunctionApp] = AzureResourceTypes.AppServicePlan,
        [AzureResourceTypes.ContainerApp] = AzureResourceTypes.ContainerAppEnvironment,
        [AzureResourceTypes.ApplicationInsights] = AzureResourceTypes.LogAnalyticsWorkspace,
        [AzureResourceTypes.SqlDatabase] = AzureResourceTypes.SqlServer,
    };

    internal static async Task<ErrorOr<(InfrastructureConfigId ConfigId, ResourceGroupId ResourceGroupId)>> CreateInfrastructureAsync(
        ISender mediator,
        Guid projectId,
        string projectName,
        string location,
        CancellationToken cancellationToken)
    {
        var configCommand = new CreateInfrastructureConfigCommand(
            Name: $"{projectName}-config",
            ProjectId: projectId);
        var configResult = await mediator.Send(configCommand, cancellationToken).ConfigureAwait(false);

        if (configResult.IsError)
            return configResult.Errors;

        var resourceGroupCommand = new CreateResourceGroupCommand(
            InfraConfigId: configResult.Value.Id,
            Name: new Name($"{projectName}-rg"),
            Location: ParseLocation(location));
        var resourceGroupResult = await mediator.Send(resourceGroupCommand, cancellationToken).ConfigureAwait(false);

        if (resourceGroupResult.IsError)
            return resourceGroupResult.Errors;

        return (configResult.Value.Id, resourceGroupResult.Value.Id);
    }

    internal static async Task<(
        List<ApplyImportPreviewCreatedResourceResult> Created,
        List<ApplyImportPreviewSkippedResourceResult> Skipped)> CreateResourcesAsync(
        ISender mediator,
        ResourceGroupId resourceGroupId,
        IReadOnlyList<ImportResourceInput> resources,
        CancellationToken cancellationToken)
    {
        var created = new List<ApplyImportPreviewCreatedResourceResult>();
        var skipped = new List<ApplyImportPreviewSkippedResourceResult>();
        var createdIdsByType = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var createdIdsByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var ordered = OrderByDependency(resources.Select(resource => (resource.ResourceType, resource.Name)));

        foreach (var (resourceType, name) in ordered)
        {
            var input = resources.FirstOrDefault(resource =>
                string.Equals(resource.ResourceType, resourceType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(resource.Name, name, StringComparison.Ordinal));

            var location = ParseLocation(input?.Location);

            var command = BuildCommand(
                resourceType,
                resourceGroupId,
                new Name(name),
                location,
                createdIdsByType,
                createdIdsByName,
                input?.DependencyResourceNames,
                input?.ExtractedProperties);

            if (command is null)
            {
                var missingDependency = GetMissingDependency(
                    resourceType,
                    createdIdsByType,
                    createdIdsByName,
                    input?.DependencyResourceNames);
                skipped.Add(new ApplyImportPreviewSkippedResourceResult(
                    resourceType,
                    name,
                    missingDependency is not null
                        ? $"Dependency '{missingDependency}' was not created or not available."
                        : $"Resource type '{resourceType}' is not supported for auto-creation."));
                continue;
            }

            try
            {
                var result = await SendResourceCommandAsync(mediator, command, cancellationToken).ConfigureAwait(false);
                var resourceId = ExtractResourceId(result);

                if (resourceId is null)
                {
                    skipped.Add(new ApplyImportPreviewSkippedResourceResult(
                        resourceType,
                        name,
                        ExtractErrors(result) ?? "Creation failed."));
                    continue;
                }

                createdIdsByType[resourceType] = resourceId.Value;
                createdIdsByName[name] = resourceId.Value;
                created.Add(new ApplyImportPreviewCreatedResourceResult(
                    resourceType,
                    resourceId.Value.ToString(),
                    name));
            }
            catch (Exception ex)
            {
                skipped.Add(new ApplyImportPreviewSkippedResourceResult(
                    resourceType,
                    name,
                    $"Unexpected error: {ex.Message}"));
            }
        }

        return (created, skipped);
    }

    internal static IReadOnlyList<(string ResourceType, string Name)> OrderByDependency(
        IEnumerable<(string ResourceType, string Name)> resources)
    {
        var list = resources.ToList();
        var typeSet = new HashSet<string>(list.Select(resource => resource.ResourceType), StringComparer.OrdinalIgnoreCase);
        var independent = new List<(string ResourceType, string Name)>();
        var dependent = new List<(string ResourceType, string Name)>();

        foreach (var resource in list)
        {
            if (ResourceDependencies.TryGetValue(resource.ResourceType, out var dependency)
                && typeSet.Contains(dependency))
            {
                dependent.Add(resource);
                continue;
            }

            independent.Add(resource);
        }

        independent.AddRange(dependent);
        return independent;
    }

    internal static string? GetMissingDependency(
        string resourceType,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid>? createdResourcesByName = null,
        IReadOnlyList<string>? dependencyResourceNames = null)
    {
        if (!ResourceDependencies.TryGetValue(resourceType, out var dependency))
            return null;

        if (dependencyResourceNames is { Count: > 0 })
        {
            var missingDependencyName = dependencyResourceNames.FirstOrDefault(name =>
                createdResourcesByName is null || !createdResourcesByName.ContainsKey(name));

            return missingDependencyName;
        }

        return !createdResourcesByType.ContainsKey(dependency)
            ? dependency
            : null;
    }

    private static IBaseRequest? BuildCommand(
        string resourceType,
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid> createdResourcesByName,
        IReadOnlyList<string>? dependencyResourceNames,
        IReadOnlyDictionary<string, object?>? extractedProperties)
    {
        return resourceType switch
        {
            AzureResourceTypes.KeyVault => new CreateKeyVaultCommand(resourceGroupId, name, location),
            AzureResourceTypes.StorageAccount => new CreateStorageAccountCommand(
                resourceGroupId,
                name,
                location,
                Kind: GetStringProp(extractedProperties, "kind", "StorageV2"),
                AccessTier: GetStringProp(extractedProperties, "accessTier", "Hot"),
                AllowBlobPublicAccess: false,
                EnableHttpsTrafficOnly: true,
                MinimumTlsVersion: "TLS1_2"),
            AzureResourceTypes.AppServicePlan => new CreateAppServicePlanCommand(
                resourceGroupId,
                name,
                location,
                OsType: GetStringProp(extractedProperties, "osType", "Linux")),
            AzureResourceTypes.WebApp => BuildWebAppCommand(resourceGroupId, name, location, createdResourcesByType, createdResourcesByName, dependencyResourceNames, extractedProperties),
            AzureResourceTypes.FunctionApp => BuildFunctionAppCommand(resourceGroupId, name, location, createdResourcesByType, createdResourcesByName, dependencyResourceNames, extractedProperties),
            AzureResourceTypes.ContainerAppEnvironment => new CreateContainerAppEnvironmentCommand(resourceGroupId, name, location),
            AzureResourceTypes.ContainerApp => BuildContainerAppCommand(resourceGroupId, name, location, createdResourcesByType, createdResourcesByName, dependencyResourceNames),
            AzureResourceTypes.RedisCache => new CreateRedisCacheCommand(
                resourceGroupId,
                name,
                location,
                RedisVersion: null,
                EnableNonSslPort: false,
                MinimumTlsVersion: "1.2",
                DisableAccessKeyAuthentication: false,
                EnableAadAuth: true),
            AzureResourceTypes.UserAssignedIdentity => new CreateUserAssignedIdentityCommand(resourceGroupId, name, location),
            AzureResourceTypes.AppConfiguration => new CreateAppConfigurationCommand(resourceGroupId, name, location),
            AzureResourceTypes.LogAnalyticsWorkspace => new CreateLogAnalyticsWorkspaceCommand(resourceGroupId, name, location),
            AzureResourceTypes.ApplicationInsights => BuildApplicationInsightsCommand(resourceGroupId, name, location, createdResourcesByType, createdResourcesByName, dependencyResourceNames),
            AzureResourceTypes.CosmosDb => new CreateCosmosDbCommand(resourceGroupId, name, location),
            AzureResourceTypes.SqlServer => new CreateSqlServerCommand(
                resourceGroupId,
                name,
                location,
                Version: "12.0",
                AdministratorLogin: "sqladmin"),
            AzureResourceTypes.SqlDatabase => BuildSqlDatabaseCommand(resourceGroupId, name, location, createdResourcesByType, createdResourcesByName, dependencyResourceNames),
            AzureResourceTypes.ServiceBusNamespace => new CreateServiceBusNamespaceCommand(resourceGroupId, name, location),
            AzureResourceTypes.ContainerRegistry => new CreateContainerRegistryCommand(resourceGroupId, name, location),
            AzureResourceTypes.EventHubNamespace => new CreateEventHubNamespaceCommand(resourceGroupId, name, location),
            _ => null,
        };
    }

    private static IBaseRequest? BuildWebAppCommand(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid> createdResourcesByName,
        IReadOnlyList<string>? dependencyResourceNames,
        IReadOnlyDictionary<string, object?>? extractedProperties)
    {
        var appServicePlanId = ResolveDependencyId(
            AzureResourceTypes.AppServicePlan,
            createdResourcesByType,
            createdResourcesByName,
            dependencyResourceNames);

        if (appServicePlanId is null)
            return null;

        return new CreateWebAppCommand(
            resourceGroupId,
            name,
            location,
            AppServicePlanId: appServicePlanId.Value,
            RuntimeStack: GetStringProp(extractedProperties, "runtimeStack", "DOTNETCORE"),
            RuntimeVersion: GetStringProp(extractedProperties, "runtimeVersion", "8.0"),
            AlwaysOn: false,
            HttpsOnly: true,
            DeploymentMode: "Zip",
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
    }

    private static IBaseRequest? BuildFunctionAppCommand(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid> createdResourcesByName,
        IReadOnlyList<string>? dependencyResourceNames,
        IReadOnlyDictionary<string, object?>? extractedProperties)
    {
        var appServicePlanId = ResolveDependencyId(
            AzureResourceTypes.AppServicePlan,
            createdResourcesByType,
            createdResourcesByName,
            dependencyResourceNames);

        if (appServicePlanId is null)
            return null;

        return new CreateFunctionAppCommand(
            resourceGroupId,
            name,
            location,
            AppServicePlanId: appServicePlanId.Value,
            RuntimeStack: GetStringProp(extractedProperties, "runtimeStack", "DOTNET-ISOLATED"),
            RuntimeVersion: GetStringProp(extractedProperties, "runtimeVersion", "8.0"),
            HttpsOnly: true,
            DeploymentMode: "Zip",
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
    }

    private static IBaseRequest? BuildContainerAppCommand(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid> createdResourcesByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        var containerAppEnvironmentId = ResolveDependencyId(
            AzureResourceTypes.ContainerAppEnvironment,
            createdResourcesByType,
            createdResourcesByName,
            dependencyResourceNames);

        if (containerAppEnvironmentId is null)
            return null;

        return new CreateContainerAppCommand(
            resourceGroupId,
            name,
            location,
            ContainerAppEnvironmentId: containerAppEnvironmentId.Value,
            ContainerRegistryId: null);
    }

    private static IBaseRequest? BuildApplicationInsightsCommand(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid> createdResourcesByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        var workspaceId = ResolveDependencyId(
            AzureResourceTypes.LogAnalyticsWorkspace,
            createdResourcesByType,
            createdResourcesByName,
            dependencyResourceNames);

        if (workspaceId is null)
            return null;

        return new CreateApplicationInsightsCommand(
            resourceGroupId,
            name,
            location,
            LogAnalyticsWorkspaceId: workspaceId.Value);
    }

    private static IBaseRequest? BuildSqlDatabaseCommand(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid> createdResourcesByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        var sqlServerId = ResolveDependencyId(
            AzureResourceTypes.SqlServer,
            createdResourcesByType,
            createdResourcesByName,
            dependencyResourceNames);

        if (sqlServerId is null)
            return null;

        return new CreateSqlDatabaseCommand(
            resourceGroupId,
            name,
            location,
            SqlServerId: sqlServerId.Value,
            Collation: "SQL_Latin1_General_CP1_CI_AS");
    }

    private static Guid? ResolveDependencyId(
        string dependencyType,
        IReadOnlyDictionary<string, Guid> createdResourcesByType,
        IReadOnlyDictionary<string, Guid> createdResourcesByName,
        IReadOnlyList<string>? dependencyResourceNames)
    {
        if (dependencyResourceNames is { Count: > 0 })
        {
            foreach (var dependencyResourceName in dependencyResourceNames)
            {
                if (createdResourcesByName.TryGetValue(dependencyResourceName, out var dependencyId))
                {
                    return dependencyId;
                }
            }

            return null;
        }

        return createdResourcesByType.TryGetValue(dependencyType, out var fallbackDependencyId)
            ? fallbackDependencyId
            : null;
    }

    private static Location ParseLocation(string? location)
    {
        return Enum.TryParse<Location.LocationEnum>(location, true, out var locationEnum)
            ? new Location(locationEnum)
            : new Location(Location.LocationEnum.WestEurope);
    }

    private static Guid? ExtractResourceId(object? result)
    {
        if (result is null)
            return null;

        var resultType = result.GetType();
        var isErrorProperty = resultType.GetProperty("IsError");
        if (isErrorProperty is not null && isErrorProperty.GetValue(result) is true)
            return null;

        var valueProperty = resultType.GetProperty("Value");
        var value = valueProperty?.GetValue(result);
        if (value is null)
            return null;

        var idProperty = value.GetType().GetProperty("Id");
        var id = idProperty?.GetValue(value);
        if (id is null)
            return null;

        var guidProperty = id.GetType().GetProperty("Value");
        var guidValue = guidProperty?.GetValue(id);

        return guidValue is Guid valueGuid ? valueGuid : null;
    }

    private static string? ExtractErrors(object? result)
    {
        if (result is null)
            return null;

        var errorsProperty = result.GetType().GetProperty("Errors");
        if (errorsProperty?.GetValue(result) is not IEnumerable<Error> errors)
            return null;

        return string.Join("; ", errors.Select(error => error.Description));
    }

    private static string GetStringProp(
        IReadOnlyDictionary<string, object?>? extractedProperties,
        string key,
        string defaultValue)
    {
        if (extractedProperties is null)
            return defaultValue;

        return extractedProperties.TryGetValue(key, out var value)
               && value is string stringValue
               && !string.IsNullOrWhiteSpace(stringValue)
            ? stringValue
            : defaultValue;
    }

    private static async Task<object?> SendResourceCommandAsync(
        ISender mediator,
        IBaseRequest command,
        CancellationToken cancellationToken)
    {
        var requestInterface = command.GetType().GetInterfaces()
            .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRequest<>));

        if (requestInterface is null)
            return null;

        var responseType = requestInterface.GetGenericArguments()[0];
        var sendMethod = typeof(ImportResourceCreationDispatcher)
            .GetMethod(nameof(SendTypedCommandAsync), BindingFlags.NonPublic | BindingFlags.Static)!;
        var genericMethod = sendMethod.MakeGenericMethod(responseType);
        var task = (Task)genericMethod.Invoke(null, [mediator, command, cancellationToken])!;
        await task.ConfigureAwait(false);

        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static async Task<TResponse> SendTypedCommandAsync<TResponse>(
        ISender mediator,
        IRequest<TResponse> command,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }
}