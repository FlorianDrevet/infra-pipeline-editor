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
        ["KeyVault"] = new("Microsoft.KeyVault/vaults", "2023-07-01", "keyVault", "keyvault", "Key Vault"),
        ["RedisCache"] = new("Microsoft.Cache/Redis", "2023-08-01", "redisCache", "redis", "Redis Cache"),
        ["StorageAccount"] = new("Microsoft.Storage/storageAccounts", "2023-01-01", "storageAccount", "storage", "Storage Account"),
        ["AppServicePlan"] = new("Microsoft.Web/serverfarms", "2023-12-01", "appServicePlan", "appserviceplan", "App Service Plan"),
        ["WebApp"] = new("Microsoft.Web/sites", "2023-12-01", "webApp", "webapp", "Web App"),
        ["FunctionApp"] = new("Microsoft.Web/sites", "2023-12-01", "functionApp", "functionapp", "Function App"),
        ["UserAssignedIdentity"] = new("Microsoft.ManagedIdentity/userAssignedIdentities", "2023-01-31", "identity", "identity", "User Assigned Identity"),
        ["AppConfiguration"] = new("Microsoft.AppConfiguration/configurationStores", "2023-03-01", "appConfig", "appconfiguration", "App Configuration"),
        ["ContainerAppEnvironment"] = new("Microsoft.App/managedEnvironments", "2024-03-01", "containerAppEnv", "containerappenvironment", "Container App Environment"),
        ["ContainerApp"] = new("Microsoft.App/containerApps", "2024-03-01", "containerApp", "containerapp", "Container App"),
        ["LogAnalyticsWorkspace"] = new("Microsoft.OperationalInsights/workspaces", "2023-09-01", "logAnalyticsWorkspace", "loganalytics", "Log Analytics Workspace"),
        ["ApplicationInsights"] = new("Microsoft.Insights/components", "2020-02-02", "applicationInsights", "applicationinsights", "Application Insights"),
        ["CosmosDb"] = new("Microsoft.DocumentDB/databaseAccounts", "2024-05-15", "cosmosDbAccount", "cosmos", "Cosmos DB"),
        ["SqlServer"] = new("Microsoft.Sql/servers", "2023-08-01-preview", "sqlServer", "sqlserver", "SQL Server"),
        ["SqlDatabase"] = new("Microsoft.Sql/servers/databases", "2023-08-01-preview", "sqlDatabase", "sqldatabase", "SQL Database"),
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
