using InfraFlowSculptor.GenerationCore;

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
        armResourceType switch
        {
            "Microsoft.KeyVault/vaults" => "2023-07-01",
            "Microsoft.Cache/Redis" => "2024-03-01",
            "Microsoft.Storage/storageAccounts" => "2023-05-01",
            "Microsoft.Web/serverfarms" => "2023-12-01",
            "Microsoft.Web/sites" => "2023-12-01",
            "Microsoft.Web/sites/functionapp" => "2023-12-01",
            "Microsoft.ManagedIdentity/userAssignedIdentities" => "2023-01-31",
            "Microsoft.AppConfiguration/configurationStores" => "2023-03-01",
            "Microsoft.App/managedEnvironments" => "2024-03-01",
            "Microsoft.App/containerApps" => "2024-03-01",
            "Microsoft.OperationalInsights/workspaces" => "2023-09-01",
            "Microsoft.Insights/components" => "2020-02-02",
            "Microsoft.DocumentDB/databaseAccounts" => "2024-05-15",
            "Microsoft.Sql/servers" => "2023-08-01-preview",
            "Microsoft.Sql/servers/databases" => "2023-08-01-preview",
            "Microsoft.ServiceBus/namespaces" => "2022-10-01-preview",
            _ => "2023-01-01"
        };

    internal static string GetBaseModuleName(string resourceType)
    {
        return resourceType switch
        {
            AzureResourceTypes.ArmTypes.KeyVault => "keyVault",
            AzureResourceTypes.ArmTypes.RedisCache => "redisCache",
            AzureResourceTypes.ArmTypes.StorageAccount => "storageAccount",
            AzureResourceTypes.ArmTypes.AppServicePlan => "appServicePlan",
            AzureResourceTypes.ArmTypes.WebApp => "webApp",
            AzureResourceTypes.ArmTypes.FunctionApp => "functionApp",
            AzureResourceTypes.ArmTypes.UserAssignedIdentity => "userAssignedIdentity",
            AzureResourceTypes.ArmTypes.AppConfiguration => "appConfiguration",
            AzureResourceTypes.ArmTypes.ContainerAppEnvironment => "containerAppEnvironment",
            AzureResourceTypes.ArmTypes.ContainerApp => "containerApp",
            AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace => "logAnalyticsWorkspace",
            AzureResourceTypes.ArmTypes.ApplicationInsights => "applicationInsights",
            AzureResourceTypes.ArmTypes.CosmosDb => "cosmosDb",
            AzureResourceTypes.ArmTypes.SqlServer => "sqlServer",
            AzureResourceTypes.ArmTypes.SqlDatabase => "sqlDatabase",
            _ => "unknown"
        };
    }
}
