using System.Diagnostics.CodeAnalysis;

namespace InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;

/// <summary>
/// Static catalog of Azure built-in RBAC role definitions grouped by AzureResource type name.
/// Role definition IDs come from the official Azure built-in roles documentation:
/// https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
/// </summary>
public static class AzureRoleDefinitionCatalog
{
    /// <summary>Well-known role definition ID for "Key Vault Secrets User".</summary>
    [SuppressMessage("SonarAnalyzer.CSharp", "S6418", Justification = "Not a secret — this is an Azure built-in RBAC role definition name.")]
    public const string KeyVaultSecretsUser = "4633458b-17de-408a-b874-0445c86b69e6";

    /// <summary>Well-known role definition ID for "Key Vault Secrets Officer".</summary>
    [SuppressMessage("SonarAnalyzer.CSharp", "S6418", Justification = "Not a secret — this is an Azure built-in RBAC role definition name.")]
    public const string KeyVaultSecretsOfficer = "b86a8fe4-44ce-4948-aee5-eccb2c155cd7";

    /// <summary>Well-known role definition ID for "Key Vault Administrator".</summary>
    public const string KeyVaultAdministrator = "00482a5a-887f-4fb3-b363-3b7fe8e74483";

    /// <summary>
    /// Set of all Azure role definition IDs that grant access to read Key Vault secrets.
    /// Used by the impact analyzer to detect broken secret references on role deletion.
    /// </summary>
    public static readonly HashSet<string> KeyVaultSecretsAccessRoles =
    [
        KeyVaultSecretsUser,
        KeyVaultSecretsOfficer,
        KeyVaultAdministrator,
    ];

    /// <summary>Well-known role definition ID for "AcrPull".</summary>
    public const string AcrPull = "7f951dda-4ed3-4680-a7ca-43fe172d538e";

    private const string KeyVaultDocsUrl =
        "https://learn.microsoft.com/en-us/azure/key-vault/general/rbac-guide";

    private const string RedisCacheDocsUrl =
        "https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-azure-active-directory-for-authentication";

    private const string StorageAccountDocsUrl =
        "https://learn.microsoft.com/en-us/azure/storage/common/storage-auth-aad-rbac-portal";

    private const string AppServicePlanDocsUrl =
        "https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity";

    private const string WebAppDocsUrl =
        "https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity";

    private static readonly IReadOnlyList<AzureRoleDefinition> KeyVaultRoles =
    [
        new("00482a5a-887f-4fb3-b363-3b7fe8e74483",
            "Key Vault Administrator",
            "Perform all data plane operations on a key vault and all objects in it, including certificates, keys, and secrets. Cannot manage key vault resources or manage role assignments.",
            KeyVaultDocsUrl),

        new("f25e0fa2-a7c8-4ddc-9c78-8a99a78b1503",
            "Key Vault Contributor",
            "Manage key vaults, but does not allow you to assign roles in Azure RBAC, and does not allow you to access secrets, keys, or certificates.",
            KeyVaultDocsUrl),

        new("21090545-7ca7-4776-b22c-e363652d74d2",
            "Key Vault Reader",
            "Read metadata of key vaults and its certificates, keys, and secrets. Cannot read sensitive values such as secret contents or key material.",
            KeyVaultDocsUrl),

        new("b86a8fe4-44ce-4948-aee5-eccb2c155cd7",
            "Key Vault Secrets Officer",
            "Perform any action on the secrets of a key vault, except manage permissions.",
            KeyVaultDocsUrl),

        new("4633458b-17de-408a-b874-0445c86b69e6",
            "Key Vault Secrets User",
            "Read secret contents including the secret portion of a certificate with private key.",
            KeyVaultDocsUrl),

        new("14b46e9e-c2b7-41b4-b07b-48a6ebf60603",
            "Key Vault Crypto Officer",
            "Perform any action on the keys of a key vault, except manage permissions.",
            KeyVaultDocsUrl),

        new("12338af0-0e69-4776-bea7-57ae8d297424",
            "Key Vault Crypto User",
            "Perform cryptographic operations using keys.",
            KeyVaultDocsUrl),

        new("e147488a-f6f5-4113-8e2d-b22465e65bf6",
            "Key Vault Crypto Service Encryption User",
            "Read metadata of keys and perform wrap/unwrap operations. Only works for key vaults that use the Azure RBAC permission model.",
            KeyVaultDocsUrl),

        new("a4417e6f-fecd-4de8-b567-7b0420556985",
            "Key Vault Certificates Officer",
            "Perform any action on the certificates of a key vault, except manage permissions.",
            KeyVaultDocsUrl),

        new("db79e9a7-68ee-4b58-9aeb-b90e7c24fcba",
            "Key Vault Certificate User",
            "Read entire certificate contents including the secret and key portion.",
            KeyVaultDocsUrl),
    ];

    private static readonly IReadOnlyList<AzureRoleDefinition> RedisCacheRoles =
    [
        new("e0f68234-74aa-48ed-b826-c38b57376e17",
            "Redis Cache Contributor",
            "Lets you manage Azure Cache for Redis instances, but not access to them.",
            RedisCacheDocsUrl),

        new("28c80f9d-8baf-46e5-8e35-86af700cfc60",
            "Redis Enterprise Cluster Data Access Administrator",
            "Full access to Azure Cache for Redis Enterprise cluster data-plane operations.",
            RedisCacheDocsUrl),

        new("1d1c5bf7-52c0-4bab-b3aa-1564e2e62540",
            "Redis Enterprise Cluster Data Reader",
            "Read-only access to Azure Cache for Redis Enterprise cluster data-plane operations.",
            RedisCacheDocsUrl),
    ];

    private static readonly IReadOnlyList<AzureRoleDefinition> StorageAccountRoles =
    [
        new("ba92f5b4-2d11-453d-a403-e96b0029c9fe",
            "Storage Blob Data Contributor",
            "Read, write, and delete Azure Storage containers and blobs.",
            StorageAccountDocsUrl),

        new("2a2b9908-6ea1-4ae2-8e65-a410df84e7d1",
            "Storage Blob Data Reader",
            "Read and list Azure Storage containers and blobs.",
            StorageAccountDocsUrl),

        new("b7e6dc6d-f1e8-4753-8033-0f276bb0955b",
            "Storage Blob Data Owner",
            "Full access to Azure Storage blob containers and data, including assigning POSIX access control.",
            StorageAccountDocsUrl),

        new("0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3",
            "Storage Table Data Contributor",
            "Read, write, and delete access to Azure Storage tables and entities.",
            StorageAccountDocsUrl),

        new("76199698-9eea-4c19-bc75-cec21354c6b6",
            "Storage Table Data Reader",
            "Read access to Azure Storage tables and entities.",
            StorageAccountDocsUrl),

        new("0c867c2a-1d8c-454a-a3db-ab2ea1bdc8bb",
            "Storage Queue Data Contributor",
            "Read, write, and delete Azure Storage queues and queue messages.",
            StorageAccountDocsUrl),

        new("19e7f393-937e-4f77-808e-94535e297925",
            "Storage Queue Data Reader",
            "Read and list Azure Storage queues and queue messages.",
            StorageAccountDocsUrl),

        new("17d1049b-9a84-46fb-8f53-869881c3d3ab",
            "Storage Account Contributor",
            "Lets you manage storage accounts, including accessing storage account keys which provide full access to storage account data.",
            StorageAccountDocsUrl),
    ];

    private static readonly IReadOnlyList<AzureRoleDefinition> AppServicePlanRoles =
    [
        new("de139f84-1756-47ae-9be6-808fbbe84772",
            "Website Contributor",
            "Lets you manage websites (not web plans), but not access to them.",
            AppServicePlanDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            AppServicePlanDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            AppServicePlanDocsUrl),
    ];

    private const string UserAssignedIdentityDocsUrl =
        "https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview";

    private static readonly IReadOnlyList<AzureRoleDefinition> UserAssignedIdentityRoles =
    [
        new("f1a07417-d97a-45cb-824c-7a7467783830",
            "Managed Identity Operator",
            "Read and assign user-assigned managed identity.",
            UserAssignedIdentityDocsUrl),
        new("e40ec5ca-96e0-45a2-b4ff-59039f2c2b59",
            "Managed Identity Contributor",
            "Create, read, update, and delete user-assigned managed identity.",
            UserAssignedIdentityDocsUrl),
    ];

    private static readonly IReadOnlyList<AzureRoleDefinition> WebAppRoles =
    [
        new("de139f84-1756-47ae-9be6-808fbbe84772",
            "Website Contributor",
            "Lets you manage websites (not web plans), but not access to them.",
            WebAppDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            WebAppDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            WebAppDocsUrl),
    ];

    private const string FunctionAppDocsUrl =
        "https://learn.microsoft.com/en-us/azure/azure-functions/functions-identity-based-connections-tutorial";

    private static readonly IReadOnlyList<AzureRoleDefinition> FunctionAppRoles =
    [
        new("de139f84-1756-47ae-9be6-808fbbe84772",
            "Website Contributor",
            "Lets you manage websites (not web plans), but not access to them.",
            FunctionAppDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            FunctionAppDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            FunctionAppDocsUrl),
    ];

    private const string AppConfigurationDocsUrl =
        "https://learn.microsoft.com/en-us/azure/azure-app-configuration/concept-enable-rbac";

    private static readonly IReadOnlyList<AzureRoleDefinition> AppConfigurationRoles =
    [
        new("516239f1-63e1-4d78-a4de-a74fb236a071",
            "App Configuration Data Reader",
            "Allows read access to App Configuration data.",
            AppConfigurationDocsUrl),

        new("5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b",
            "App Configuration Data Owner",
            "Allows full access to App Configuration data.",
            AppConfigurationDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            AppConfigurationDocsUrl),
    ];

    private const string ContainerAppEnvironmentDocsUrl =
        "https://learn.microsoft.com/en-us/azure/container-apps/managed-identity";

    private static readonly IReadOnlyList<AzureRoleDefinition> ContainerAppEnvironmentRoles =
    [
        new("12710879-3cda-4e3e-8c7c-bf6d0cae531e",
            "ContainerApp Environment Contributor",
            "Manage Container App Environments.",
            ContainerAppEnvironmentDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            ContainerAppEnvironmentDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            ContainerAppEnvironmentDocsUrl),
    ];

    private const string ContainerAppDocsUrl =
        "https://learn.microsoft.com/en-us/azure/container-apps/managed-identity";

    private static readonly IReadOnlyList<AzureRoleDefinition> ContainerAppRoles =
    [
        new("854e5e7e-28f7-4e24-bc87-eb5c2280195d",
            "ContainerApp Contributor",
            "Manage Container Apps.",
            ContainerAppDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            ContainerAppDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            ContainerAppDocsUrl),
    ];

    private const string LogAnalyticsWorkspaceDocsUrl =
        "https://learn.microsoft.com/en-us/azure/azure-monitor/logs/manage-access";

    private static readonly IReadOnlyList<AzureRoleDefinition> LogAnalyticsWorkspaceRoles =
    [
        new("92aaf0da-9dab-42b6-94a3-d43ce8d16293",
            "Log Analytics Contributor",
            "Can read all monitoring data and edit monitoring settings. Includes managing Log Analytics workspaces.",
            LogAnalyticsWorkspaceDocsUrl),

        new("73c42c96-874c-492b-b04d-ab87d138a893",
            "Log Analytics Reader",
            "Can view and search all monitoring data as well as view monitoring settings.",
            LogAnalyticsWorkspaceDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            LogAnalyticsWorkspaceDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            LogAnalyticsWorkspaceDocsUrl),
    ];

    private const string CosmosDbDocsUrl =
        "https://learn.microsoft.com/en-us/azure/cosmos-db/role-based-access-control";

    private static readonly IReadOnlyList<AzureRoleDefinition> CosmosDbRoles =
    [
        new("fbdf93bf-df7d-467e-a4d2-9458aa1360c8",
            "Cosmos DB Account Reader Role",
            "Can read Azure Cosmos DB account data.",
            CosmosDbDocsUrl),

        new("5bd9cd88-fe45-4216-938b-f97437e15450",
            "DocumentDB Account Contributor",
            "Can manage Azure Cosmos DB accounts.",
            CosmosDbDocsUrl),

        new("230815da-be43-4aae-9cb4-875f7bd000aa",
            "Cosmos DB Operator",
            "Lets you manage Azure Cosmos DB accounts, but not access data in them.",
            CosmosDbDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            CosmosDbDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            CosmosDbDocsUrl),
    ];

    private const string ApplicationInsightsDocsUrl =
        "https://learn.microsoft.com/en-us/azure/azure-monitor/app/resources-roles-access-control";

    private static readonly IReadOnlyList<AzureRoleDefinition> ApplicationInsightsRoles =
    [
        new("ae349356-3a1b-4a5e-921d-050484c6347e",
            "Application Insights Component Contributor",
            "Can manage Application Insights components.",
            ApplicationInsightsDocsUrl),

        new("749f88d5-cbae-40b8-bcfc-e573ddc772fa",
            "Monitoring Contributor",
            "Can read all monitoring data and update monitoring settings.",
            ApplicationInsightsDocsUrl),

        new("43d0d8ad-25c7-4714-9337-8ba259a9fe05",
            "Monitoring Reader",
            "Can read all monitoring data.",
            ApplicationInsightsDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            ApplicationInsightsDocsUrl),
    ];

    private const string SqlServerDocsUrl =
        "https://learn.microsoft.com/en-us/azure/azure-sql/database/security-overview";

    private static readonly IReadOnlyList<AzureRoleDefinition> SqlServerRoles =
    [
        new("6d8ee4ec-f05a-4a1d-8b00-a9b17e38b437",
            "SQL Server Contributor",
            "Lets you manage SQL servers and databases, but not access to them, and not their security-related policies.",
            SqlServerDocsUrl),

        new("056cd41c-7e88-42e1-933e-a3ba3f36885e",
            "SQL Security Manager",
            "Lets you manage the security-related policies of SQL servers and databases, but not access to them.",
            SqlServerDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            SqlServerDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            SqlServerDocsUrl),
    ];

    private const string ServiceBusNamespaceDocsUrl =
        "https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-managed-service-identity";

    private static readonly IReadOnlyList<AzureRoleDefinition> ServiceBusNamespaceRoles =
    [
        new("090c5cfd-751d-490a-894a-3ce6f1109419",
            "Azure Service Bus Data Owner",
            "Allows for full access to Azure Service Bus resources.",
            ServiceBusNamespaceDocsUrl),

        new("69a216fc-b8fb-44d8-bc22-1f3c2cd27a39",
            "Azure Service Bus Data Sender",
            "Allows for send access to Azure Service Bus resources.",
            ServiceBusNamespaceDocsUrl),

        new("4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0",
            "Azure Service Bus Data Receiver",
            "Allows for receive access to Azure Service Bus resources.",
            ServiceBusNamespaceDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            ServiceBusNamespaceDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            ServiceBusNamespaceDocsUrl),
    ];

    private const string ContainerRegistryDocsUrl =
        "https://learn.microsoft.com/en-us/azure/container-registry/container-registry-roles";

    private const string EventHubNamespaceDocsUrl =
        "https://learn.microsoft.com/en-us/azure/event-hubs/authorize-access-azure-active-directory";

    private static readonly IReadOnlyList<AzureRoleDefinition> EventHubNamespaceRoles =
    [
        new("f526a384-b230-433a-b45c-95f59c4a2dec",
            "Azure Event Hubs Data Owner",
            "Allows for full access to Azure Event Hubs resources.",
            EventHubNamespaceDocsUrl),

        new("2b629674-e913-4c01-ae53-ef4638d8f975",
            "Azure Event Hubs Data Sender",
            "Allows send access to Azure Event Hubs resources.",
            EventHubNamespaceDocsUrl),

        new("a638d3c7-ab3a-418d-83e6-5f17a39d4fde",
            "Azure Event Hubs Data Receiver",
            "Allows receive access to Azure Event Hubs resources.",
            EventHubNamespaceDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            EventHubNamespaceDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            EventHubNamespaceDocsUrl),
    ];

    /// <summary>Azure RBAC role definitions for Container Registry.</summary>
    private static readonly IReadOnlyList<AzureRoleDefinition> ContainerRegistryRoles =
    [
        new("7f951dda-4ed3-4680-a7ca-43fe172d538d",
            "AcrPush",
            "Allows push and pull of images to an Azure Container Registry.",
            ContainerRegistryDocsUrl),

        new("7f951dda-4ed3-4680-a7ca-43fe172d538e",
            "AcrPull",
            "Allows pull of images from an Azure Container Registry.",
            ContainerRegistryDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            ContainerRegistryDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            ContainerRegistryDocsUrl),
    ];

    private const string SqlDatabaseDocsUrl =
        "https://learn.microsoft.com/en-us/azure/azure-sql/database/security-overview";

    private static readonly IReadOnlyList<AzureRoleDefinition> SqlDatabaseRoles =
    [
        new("9b7fa17d-e63e-47b0-bb0a-15c516ac86ec",
            "SQL DB Contributor",
            "Lets you manage SQL databases, but not access to them. Also, you can't manage their security-related policies or their parent SQL servers.",
            SqlDatabaseDocsUrl),

        new("6d8ee4ec-f05a-4a1d-8b00-a9b17e38b437",
            "SQL Server Contributor",
            "Lets you manage SQL servers and databases, but not access to them, and not their security-related policies.",
            SqlDatabaseDocsUrl),

        new("b24988ac-6180-42a0-ab88-20f7382dd24c",
            "Contributor",
            "Full management access to all resources, but does not allow you to assign roles in Azure RBAC.",
            SqlDatabaseDocsUrl),

        new("acdd72a7-3385-48ef-bd42-f606fba81ae7",
            "Reader",
            "View all resources, but does not allow you to make any changes.",
            SqlDatabaseDocsUrl),
    ];

    private static readonly Dictionary<string, IReadOnlyList<AzureRoleDefinition>> Catalog = new()
    {
        { "KeyVault", KeyVaultRoles },
        { "RedisCache", RedisCacheRoles },
        { "StorageAccount", StorageAccountRoles },
        { "AppServicePlan", AppServicePlanRoles },
        { "WebApp", WebAppRoles },
        { "FunctionApp", FunctionAppRoles },
        { "UserAssignedIdentity", UserAssignedIdentityRoles },
        { "AppConfiguration", AppConfigurationRoles },
        { "ContainerAppEnvironment", ContainerAppEnvironmentRoles },
        { "ContainerApp", ContainerAppRoles },
        { "LogAnalyticsWorkspace", LogAnalyticsWorkspaceRoles },
        { "ApplicationInsights", ApplicationInsightsRoles },
        { "CosmosDb", CosmosDbRoles },
        { "SqlServer", SqlServerRoles },
        { "SqlDatabase", SqlDatabaseRoles },
        { "ServiceBusNamespace", ServiceBusNamespaceRoles },
        { "ContainerRegistry", ContainerRegistryRoles },
        { "EventHubNamespace", EventHubNamespaceRoles },
    };

    /// <summary>Returns all available role definitions for the given resource type name.</summary>
    public static IReadOnlyList<AzureRoleDefinition> GetForResourceType(string resourceType) =>
        Catalog.TryGetValue(resourceType, out var roles) ? roles : Array.Empty<AzureRoleDefinition>();

    /// <summary>Returns true when <paramref name="roleDefinitionId"/> is in the catalog for the given resource type.</summary>
    public static bool IsValidForResourceType(string resourceType, string roleDefinitionId) =>
        GetForResourceType(resourceType)
            .Any(r => r.Id.Equals(roleDefinitionId, StringComparison.OrdinalIgnoreCase));
}
