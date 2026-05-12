using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Provides Bicep role assignment module templates for each Azure resource type.
/// Each template uses <c>existing</c> resource references and <c>RbacRoleType</c> from types.bicep.
/// Only modules for resource types that actually have role assignments are generated.
/// </summary>
public static class RoleAssignmentModuleTemplates
{
    /// <summary>
    /// Metadata required to generate a role assignment module for a given resource type.
    /// </summary>
    public sealed record ResourceTypeMetadata(
        string AzureResourceType,
        string ApiVersion,
        string BicepSymbol,
        string ServiceCategory,
        string Description);

    /// <summary>
    /// Mapping from the simple resource type name (e.g. "KeyVault") to its Bicep metadata.
    /// </summary>
    private static readonly Dictionary<string, ResourceTypeMetadata> Metadata = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.KeyVault] = new(AzureResourceTypes.ArmTypes.KeyVaultType, "2023-07-01", "keyVault", "keyvault", "Key Vault"),
        [AzureResourceTypes.RedisCache] = new(AzureResourceTypes.ArmTypes.RedisCacheType, "2023-08-01", "redisCache", "redis", "Redis Cache"),
        [AzureResourceTypes.StorageAccount] = new(AzureResourceTypes.ArmTypes.StorageAccountType, "2023-01-01", "storageAccount", "storage", "Storage Account"),
        [AzureResourceTypes.AppServicePlan] = new(AzureResourceTypes.ArmTypes.AppServicePlanType, "2023-12-01", "appServicePlan", "appserviceplan", "App Service Plan"),
        [AzureResourceTypes.WebApp] = new(AzureResourceTypes.ArmTypes.WebAppType, "2023-12-01", "webApp", "webapp", "Web App"),
        [AzureResourceTypes.FunctionApp] = new(AzureResourceTypes.ArmTypes.WebAppType, "2023-12-01", "functionApp", "functionapp", "Function App"),
        [AzureResourceTypes.UserAssignedIdentity] = new(AzureResourceTypes.ArmTypes.UserAssignedIdentityType, "2023-01-31", "identity", "identity", "User Assigned Identity"),
        [AzureResourceTypes.AppConfiguration] = new(AzureResourceTypes.ArmTypes.AppConfigurationType, "2023-03-01", "appConfig", "appconfiguration", "App Configuration"),
        [AzureResourceTypes.ContainerAppEnvironment] = new(AzureResourceTypes.ArmTypes.ContainerAppEnvironmentType, "2024-03-01", "containerAppEnv", "containerappenvironment", "Container App Environment"),
        [AzureResourceTypes.ContainerApp] = new(AzureResourceTypes.ArmTypes.ContainerAppType, "2024-03-01", "containerApp", "containerapp", "Container App"),
        [AzureResourceTypes.LogAnalyticsWorkspace] = new(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType, "2023-09-01", "logAnalyticsWorkspace", "loganalytics", "Log Analytics Workspace"),
        [AzureResourceTypes.ApplicationInsights] = new(AzureResourceTypes.ArmTypes.ApplicationInsightsType, "2020-02-02", "applicationInsights", "applicationinsights", "Application Insights"),
        [AzureResourceTypes.CosmosDb] = new(AzureResourceTypes.ArmTypes.CosmosDbType, "2024-05-15", "cosmosDbAccount", "cosmos", "Cosmos DB"),
        [AzureResourceTypes.SqlServer] = new(AzureResourceTypes.ArmTypes.SqlServerType, "2023-08-01-preview", "sqlServer", "sqlserver", "SQL Server"),
        [AzureResourceTypes.SqlDatabase] = new(AzureResourceTypes.ArmTypes.SqlDatabaseType, "2023-08-01-preview", "sqlDatabase", "sqldatabase", "SQL Database"),
        [AzureResourceTypes.ServiceBusNamespace] = new(AzureResourceTypes.ArmTypes.ServiceBusNamespaceType, "2022-10-01-preview", "serviceBusNamespace", "servicebus", "Service Bus Namespace"),
        [AzureResourceTypes.ContainerRegistry] = new(AzureResourceTypes.ArmTypes.ContainerRegistryType, "2023-07-01", "containerRegistry", "containerregistry", "Container Registry"),
        [AzureResourceTypes.EventHubNamespace] = new(AzureResourceTypes.ArmTypes.EventHubNamespaceType, "2024-01-01", "eventHubNamespace", "eventhub", "Event Hub Namespace"),
    };

    /// <summary>
    /// Returns the <see cref="ResourceTypeMetadata"/> for the given simple resource type name.
    /// </summary>
    public static ResourceTypeMetadata? GetMetadata(string resourceTypeName) =>
        Metadata.GetValueOrDefault(resourceTypeName);

    /// <summary>
    /// Returns the service category key for a given resource type name (e.g. "KeyVault" → "keyvault").
    /// </summary>
    public static string GetServiceCategory(string resourceTypeName) =>
        Metadata.TryGetValue(resourceTypeName, out var meta) ? meta.ServiceCategory : resourceTypeName.ToLowerInvariant();

    /// <summary>
    /// Generates the Bicep role assignment module content for the given resource type.
    /// </summary>
    public static string GenerateModule(string resourceTypeName)
    {
        if (!Metadata.TryGetValue(resourceTypeName, out var meta))
            throw new NotSupportedException($"Role assignment module not supported for resource type '{resourceTypeName}'.");

        return $$"""
            // =======================================================================
            // {{meta.Description}} Role Assignment Module
            // -----------------------------------------------------------------------
            // Module: {{meta.ServiceCategory}}.roleassignments.module.bicep
            // Description: Creates role assignments for {{meta.Description}} resources
            // See: https://learn.microsoft.com/en-us/azure/templates/microsoft.authorization/roleassignments
            // =======================================================================

            import { RbacRoleType } from '../../types.bicep'

            @description('The name of the {{meta.Description}} instance')
            param name string

            @description('The principal ID to assign the role to')
            param principalId string

            @description('The roles to assign to the principal')
            param roles RbacRoleType[]

            resource {{meta.BicepSymbol}} '{{meta.AzureResourceType}}@{{meta.ApiVersion}}' existing = {
              name: name
            }

            resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in roles: {
              scope: {{meta.BicepSymbol}}
              name: guid({{meta.BicepSymbol}}.id, principalId, role.id)
              properties: {
                roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', role.id)
                principalId: principalId
                description: role.description
              }
            }]
            """;
    }

    /// <summary>
    /// Returns the module file name for a given resource type (e.g. "KeyVault" → "keyvault.roleassignments.module.bicep").
    /// </summary>
    public static string GetModuleFileName(string resourceTypeName)
    {
        var meta = GetMetadata(resourceTypeName)
            ?? throw new NotSupportedException($"Resource type '{resourceTypeName}' not supported.");
        return $"{meta.ServiceCategory}.roleassignments.module.bicep";
    }
}
