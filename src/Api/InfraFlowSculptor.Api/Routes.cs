namespace InfraFlowSculptor.Api;

/// <summary>
/// Centralized route constants for all API endpoints.
/// Eliminates magic strings in controller MapGroup/MapGet/MapPost/MapPut/MapDelete calls.
/// </summary>
public static class Routes
{
    /// <summary>Route group prefix for project operations.</summary>
    public const string Projects = "/projects";

    /// <summary>Route group prefix for infrastructure configuration operations.</summary>
    public const string InfraConfig = "/infra-config";

    /// <summary>Route group prefix for naming template operations scoped to an infra config.</summary>
    public const string InfraConfigNaming = "/infra-config/{id:guid}/naming";

    /// <summary>Route group prefix for standalone naming operations.</summary>
    public const string Naming = "/naming";

    /// <summary>Route group prefix for resource group operations.</summary>
    public const string ResourceGroup = "/resource-group";

    /// <summary>Route group prefix for Key Vault operations.</summary>
    public const string KeyVault = "/keyvault";

    /// <summary>Route group prefix for Redis Cache operations.</summary>
    public const string RedisCache = "/redis-cache";

    /// <summary>Route group prefix for Storage Account operations.</summary>
    public const string StorageAccounts = "/storage-accounts";

    /// <summary>Route group prefix for App Service Plan operations.</summary>
    public const string AppServicePlan = "/app-service-plan";

    /// <summary>Route group prefix for Web App operations.</summary>
    public const string WebApp = "/web-app";

    /// <summary>Route group prefix for Function App operations.</summary>
    public const string FunctionApp = "/function-app";

    /// <summary>Route group prefix for User Assigned Identity operations.</summary>
    public const string UserAssignedIdentity = "/user-assigned-identity";

    /// <summary>Route group prefix for App Configuration operations.</summary>
    public const string AppConfiguration = "/app-configuration";

    /// <summary>Route group prefix for Container App Environment operations.</summary>
    public const string ContainerAppEnvironment = "/container-app-environment";

    /// <summary>Route group prefix for Container App operations.</summary>
    public const string ContainerApp = "/container-app";

    /// <summary>Route group prefix for Log Analytics Workspace operations.</summary>
    public const string LogAnalyticsWorkspace = "/log-analytics-workspace";

    /// <summary>Route group prefix for Application Insights operations.</summary>
    public const string ApplicationInsights = "/application-insights";

    /// <summary>Route group prefix for Cosmos DB operations.</summary>
    public const string CosmosDb = "/cosmos-db";

    /// <summary>Route group prefix for SQL Server operations.</summary>
    public const string SqlServer = "/sql-server";

    /// <summary>Route group prefix for SQL Database operations.</summary>
    public const string SqlDatabase = "/sql-database";

    /// <summary>Route group prefix for Service Bus Namespace operations.</summary>
    public const string ServiceBus = "/service-bus";

    /// <summary>Route group prefix for Container Registry operations.</summary>
    public const string ContainerRegistry = "/container-registry";

    /// <summary>Route group prefix for Event Hub Namespace operations.</summary>
    public const string EventHubs = "/event-hubs";

    /// <summary>Route group prefix for Document Intelligence operations.</summary>
    public const string DocumentIntelligence = "/document-intelligence";

    /// <summary>Route group prefix for Virtual Network operations.</summary>
    public const string VirtualNetwork = "/virtual-network";

    /// <summary>Route group prefix for Network Security Group operations.</summary>
    public const string NetworkSecurityGroup = "/network-security-group";

    /// <summary>Route group prefix for Private DNS Zone operations.</summary>
    public const string PrivateDnsZone = "/private-dns-zone";

    /// <summary>Route group prefix for Front Door operations.</summary>
    public const string FrontDoor = "/front-door";

    /// <summary>Route group prefix for Personal Access Token operations.</summary>
    public const string PersonalAccessTokens = "/personal-access-tokens";

    /// <summary>Route group prefix for Bicep generation operations.</summary>
    public const string GenerateBicep = "/generate-bicep";

    /// <summary>Route group prefix for pipeline generation operations.</summary>
    public const string GeneratePipeline = "/generate-pipeline";

    /// <summary>Route group prefix for import operations.</summary>
    public const string Imports = "/imports";

    /// <summary>Route group prefix for role assignment operations scoped to a resource.</summary>
    public const string AzureResourceRoleAssignments = "/azure-resources/{resourceId:guid}/role-assignments";

    /// <summary>Route group prefix for assigned identity operations scoped to a resource.</summary>
    public const string AzureResourceAssignedIdentity = "/azure-resources/{resourceId:guid}/assigned-identity";

    /// <summary>Route group prefix for app setting operations scoped to a resource.</summary>
    public const string AzureResourceAppSettings = "/azure-resources/{resourceId:guid}/app-settings";

    /// <summary>Route group prefix for secure parameter mapping operations scoped to a resource.</summary>
    public const string AzureResourceSecureParameterMappings = "/azure-resources/{resourceId:guid}/secure-parameter-mappings";

    /// <summary>Route group prefix for custom domain operations scoped to a resource.</summary>
    public const string AzureResourceCustomDomains = "/azure-resources/{resourceId:guid}/custom-domains";

    /// <summary>Route group prefix for private endpoint operations scoped to a resource.</summary>
    public const string ResourcePrivateEndpoints = "/resources/{resourceId:guid}/private-endpoints";

    /// <summary>Route group prefix for available outputs scoped to a resource.</summary>
    public const string AzureResourceAvailableOutputs = "/azure-resources/{resourceId:guid}/available-outputs";

    /// <summary>Route group prefix for Key Vault access check scoped to a resource.</summary>
    public const string AzureResourceCheckKeyVaultAccess = "/azure-resources/{resourceId:guid}/check-keyvault-access";

    /// <summary>Route group prefix for ACR pull access check scoped to a resource.</summary>
    public const string AzureResourceCheckAcrPullAccess = "/azure-resources/{resourceId:guid}/check-acr-pull-access";

    /// <summary>Route group prefix for App Configuration key operations.</summary>
    public const string AzureResourceConfigurationKeys = "/azure-resources/{appConfigurationId:guid}/configuration-keys";

    /// <summary>Route for auto-detecting pipeline options from the repository associated with a resource.</summary>
    public const string AzureResourceDetectPipelineOptions = "/azure-resources/{resourceId:guid}/detect-pipeline-options";

    /// <summary>Route group prefix for stateless Git operations.</summary>
    public const string Git = "/git";

    /// <summary>Route group prefix for reference catalog endpoints.</summary>
    public const string Catalogs = "/catalogs";
}
