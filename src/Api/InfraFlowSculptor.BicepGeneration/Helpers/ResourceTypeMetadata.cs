using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.BicepGeneration.Constants;

namespace InfraFlowSculptor.BicepGeneration.Helpers;

/// <summary>
/// Maps resource type names to folder names, display names, documentation URLs,
/// ARM API versions, and base module names.
/// </summary>
internal static class ResourceTypeMetadata
{
    internal static string GetModuleFolderName(string resourceTypeName) =>
        resourceTypeName switch
        {
            AzureResourceTypes.KeyVault => AzureResourceTypes.KeyVault,
            AzureResourceTypes.RedisCache => AzureResourceTypes.RedisCache,
            AzureResourceTypes.StorageAccount => AzureResourceTypes.StorageAccount,
            AzureResourceTypes.AppServicePlan => AzureResourceTypes.AppServicePlan,
            AzureResourceTypes.WebApp => AzureResourceTypes.WebApp,
            AzureResourceTypes.FunctionApp => AzureResourceTypes.FunctionApp,
            AzureResourceTypes.UserAssignedIdentity => AzureResourceTypes.UserAssignedIdentity,
            AzureResourceTypes.AppConfiguration => AzureResourceTypes.AppConfiguration,
            AzureResourceTypes.ContainerAppEnvironment => AzureResourceTypes.ContainerAppEnvironment,
            AzureResourceTypes.ContainerApp => AzureResourceTypes.ContainerApp,
            AzureResourceTypes.LogAnalyticsWorkspace => AzureResourceTypes.LogAnalyticsWorkspace,
            AzureResourceTypes.ApplicationInsights => AzureResourceTypes.ApplicationInsights,
            AzureResourceTypes.CosmosDb => AzureResourceTypes.CosmosDb,
            AzureResourceTypes.SqlServer => AzureResourceTypes.SqlServer,
            AzureResourceTypes.SqlDatabase => AzureResourceTypes.SqlDatabase,
            _ => resourceTypeName
        };

    /// <summary>
    /// Returns a human-readable display name for the resource type.
    /// </summary>
    internal static string GetResourceTypeDisplayName(string resourceTypeName) =>
        resourceTypeName switch
        {
            AzureResourceTypes.KeyVault => "Key Vault",
            AzureResourceTypes.RedisCache => "Redis Cache",
            AzureResourceTypes.StorageAccount => "Storage Account",
            AzureResourceTypes.AppServicePlan => "App Service Plan",
            AzureResourceTypes.WebApp => "Web App",
            AzureResourceTypes.FunctionApp => "Function App",
            AzureResourceTypes.UserAssignedIdentity => "User Assigned Identity",
            AzureResourceTypes.AppConfiguration => "App Configuration",
            AzureResourceTypes.ContainerAppEnvironment => "Container App Environment",
            AzureResourceTypes.ContainerApp => "Container App",
            AzureResourceTypes.LogAnalyticsWorkspace => "Log Analytics Workspace",
            AzureResourceTypes.ApplicationInsights => "Application Insights",
            AzureResourceTypes.CosmosDb => "Cosmos DB",
            AzureResourceTypes.SqlServer => "SQL Server",
            AzureResourceTypes.SqlDatabase => "SQL Database",
            AzureResourceTypes.ServiceBusNamespace => "Service Bus Namespace",
            _ => resourceTypeName
        };

    /// <summary>
    /// Returns the Microsoft Learn documentation URL for the resource type.
    /// </summary>
    internal static string GetResourceTypeDocumentationUrl(string resourceTypeName) =>
        resourceTypeName switch
        {
            AzureResourceTypes.KeyVault => "https://learn.microsoft.com/en-us/azure/templates/microsoft.keyvault/vaults",
            AzureResourceTypes.RedisCache => "https://learn.microsoft.com/en-us/azure/templates/microsoft.cache/redis",
            AzureResourceTypes.StorageAccount => "https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts",
            AzureResourceTypes.AppServicePlan => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/serverfarms",
            AzureResourceTypes.WebApp => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/sites",
            AzureResourceTypes.FunctionApp => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/sites",
            AzureResourceTypes.UserAssignedIdentity => "https://learn.microsoft.com/en-us/azure/templates/microsoft.managedidentity/userassignedidentities",
            AzureResourceTypes.AppConfiguration => "https://learn.microsoft.com/en-us/azure/templates/microsoft.appconfiguration/configurationstores",
            AzureResourceTypes.ContainerAppEnvironment => "https://learn.microsoft.com/en-us/azure/templates/microsoft.app/managedenvironments",
            AzureResourceTypes.ContainerApp => "https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps",
            AzureResourceTypes.LogAnalyticsWorkspace => "https://learn.microsoft.com/en-us/azure/templates/microsoft.operationalinsights/workspaces",
            AzureResourceTypes.ApplicationInsights => "https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/components",
            AzureResourceTypes.CosmosDb => "https://learn.microsoft.com/en-us/azure/templates/microsoft.documentdb/databaseaccounts",
            AzureResourceTypes.SqlServer => "https://learn.microsoft.com/en-us/azure/templates/microsoft.sql/servers",
            AzureResourceTypes.SqlDatabase => "https://learn.microsoft.com/en-us/azure/templates/microsoft.sql/servers/databases",
            AzureResourceTypes.ServiceBusNamespace => "https://learn.microsoft.com/en-us/azure/templates/microsoft.servicebus/namespaces",
            _ => string.Empty
        };

    /// <summary>
    /// Returns the API version to use for <c>existing</c> resource declarations by ARM resource type.
    /// </summary>
    internal static string GetExistingResourceApiVersion(string armResourceType) =>
        BicepArmTypeCatalog.GetExistingResourceApiVersion(armResourceType);

    internal static string GetBaseModuleName(string resourceType)
    {
        return resourceType switch
        {
            AzureResourceTypes.ArmTypes.KeyVaultType => "keyVault",
            AzureResourceTypes.ArmTypes.RedisCacheType => "redisCache",
            AzureResourceTypes.ArmTypes.StorageAccountType => "storageAccount",
            AzureResourceTypes.ArmTypes.AppServicePlanType => "appServicePlan",
            AzureResourceTypes.ArmTypes.WebAppType => "webApp",
            AzureResourceTypes.ArmTypes.FunctionAppType => "functionApp",
            AzureResourceTypes.ArmTypes.UserAssignedIdentityType => "userAssignedIdentity",
            AzureResourceTypes.ArmTypes.AppConfigurationType => "appConfiguration",
            AzureResourceTypes.ArmTypes.ContainerAppEnvironmentType => "containerAppEnvironment",
            AzureResourceTypes.ArmTypes.ContainerAppType => "containerApp",
            AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType => "logAnalyticsWorkspace",
            AzureResourceTypes.ArmTypes.ApplicationInsightsType => "applicationInsights",
            AzureResourceTypes.ArmTypes.CosmosDbType => "cosmosDb",
            AzureResourceTypes.ArmTypes.SqlServerType => "sqlServer",
            AzureResourceTypes.ArmTypes.SqlDatabaseType => "sqlDatabase",
            AzureResourceTypes.ArmTypes.ServiceBusNamespaceType => "serviceBusNamespace",
            AzureResourceTypes.ArmTypes.ContainerRegistryType => "containerRegistry",
            AzureResourceTypes.ArmTypes.EventHubNamespaceType => "eventHubNamespace",
            _ => "unknown"
        };
    }
}
