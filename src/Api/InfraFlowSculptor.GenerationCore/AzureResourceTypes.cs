using System.Collections.Frozen;

namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Centralized constants for Azure resource type identifiers used across generation engines.
/// Provides friendly type names, ARM resource type strings, and the mapping between them.
/// </summary>
public static class AzureResourceTypes
{
    // ── Friendly type names (compile-time constants for switch patterns, dictionary keys, etc.) ──

    public const string KeyVault = "KeyVault";
    public const string RedisCache = "RedisCache";
    public const string StorageAccount = "StorageAccount";
    public const string AppServicePlan = "AppServicePlan";
    public const string WebApp = "WebApp";
    public const string FunctionApp = "FunctionApp";
    public const string UserAssignedIdentity = "UserAssignedIdentity";
    public const string AppConfiguration = "AppConfiguration";
    public const string ContainerAppEnvironment = "ContainerAppEnvironment";
    public const string ContainerApp = "ContainerApp";
    public const string LogAnalyticsWorkspace = "LogAnalyticsWorkspace";
    public const string ApplicationInsights = "ApplicationInsights";
    public const string CosmosDb = "CosmosDb";
    public const string SqlServer = "SqlServer";
    public const string SqlDatabase = "SqlDatabase";
    public const string ServiceBusNamespace = "ServiceBusNamespace";
    public const string ContainerRegistry = "ContainerRegistry";
    public const string EventHubNamespace = "EventHubNamespace";
    public const string ResourceGroup = "ResourceGroup";
    public const string VirtualNetwork = "VirtualNetwork";
    public const string NetworkSecurityGroup = "NetworkSecurityGroup";
    public const string PrivateDnsZone = "PrivateDnsZone";
    public const string FrontDoor = "FrontDoor";
    public const string DocumentIntelligence = "DocumentIntelligence";

    /// <summary>
    /// Azure ARM resource provider type strings (e.g. "Microsoft.KeyVault/vaults").
    /// </summary>
    public static class ArmTypes
    {
        public const string KeyVaultType = "Microsoft.KeyVault/vaults";
        public const string RedisCacheType = "Microsoft.Cache/Redis";
        public const string StorageAccountType = "Microsoft.Storage/storageAccounts";
        public const string AppServicePlanType = "Microsoft.Web/serverfarms";
        public const string WebAppType = "Microsoft.Web/sites";
        public const string FunctionAppType = "Microsoft.Web/sites/functionapp";
        public const string UserAssignedIdentityType = "Microsoft.ManagedIdentity/userAssignedIdentities";
        public const string AppConfigurationType = "Microsoft.AppConfiguration/configurationStores";
        public const string ContainerAppEnvironmentType = "Microsoft.App/managedEnvironments";
        public const string ContainerAppType = "Microsoft.App/containerApps";
        public const string LogAnalyticsWorkspaceType = "Microsoft.OperationalInsights/workspaces";
        public const string ApplicationInsightsType = "Microsoft.Insights/components";
        public const string CosmosDbType = "Microsoft.DocumentDB/databaseAccounts";
        public const string SqlServerType = "Microsoft.Sql/servers";
        public const string SqlDatabaseType = "Microsoft.Sql/servers/databases";
        public const string ServiceBusNamespaceType = "Microsoft.ServiceBus/namespaces";
        public const string ContainerRegistryType = "Microsoft.ContainerRegistry/registries";
        public const string EventHubNamespaceType = "Microsoft.EventHub/namespaces";
        public const string VirtualNetworkType = "Microsoft.Network/virtualNetworks";
        public const string NetworkSecurityGroupType = "Microsoft.Network/networkSecurityGroups";
        public const string PrivateDnsZoneType = "Microsoft.Network/privateDnsZones";
        public const string FrontDoorType = "Microsoft.Cdn/profiles";
        public const string PrivateEndpointType = "Microsoft.Network/privateEndpoints";
        public const string DocumentIntelligenceType = "Microsoft.CognitiveServices/accounts";
    }

    /// <summary>
    /// Maps Azure ARM resource type strings to their friendly type names.
    /// Case-insensitive lookup.
    /// </summary>
    private static readonly FrozenDictionary<string, string> ArmTypeToFriendlyNameMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ArmTypes.KeyVaultType] = KeyVault,
            [ArmTypes.RedisCacheType] = RedisCache,
            [ArmTypes.StorageAccountType] = StorageAccount,
            [ArmTypes.AppServicePlanType] = AppServicePlan,
            [ArmTypes.WebAppType] = WebApp,
            [ArmTypes.FunctionAppType] = FunctionApp,
            [ArmTypes.UserAssignedIdentityType] = UserAssignedIdentity,
            [ArmTypes.AppConfigurationType] = AppConfiguration,
            [ArmTypes.ContainerAppEnvironmentType] = ContainerAppEnvironment,
            [ArmTypes.ContainerAppType] = ContainerApp,
            [ArmTypes.LogAnalyticsWorkspaceType] = LogAnalyticsWorkspace,
            [ArmTypes.ApplicationInsightsType] = ApplicationInsights,
            [ArmTypes.CosmosDbType] = CosmosDb,
            [ArmTypes.SqlServerType] = SqlServer,
            [ArmTypes.SqlDatabaseType] = SqlDatabase,
            [ArmTypes.ServiceBusNamespaceType] = ServiceBusNamespace,
            [ArmTypes.ContainerRegistryType] = ContainerRegistry,
            [ArmTypes.EventHubNamespaceType] = EventHubNamespace,
            [ArmTypes.VirtualNetworkType] = VirtualNetwork,
            [ArmTypes.NetworkSecurityGroupType] = NetworkSecurityGroup,
            [ArmTypes.PrivateDnsZoneType] = PrivateDnsZone,
            [ArmTypes.FrontDoorType] = FrontDoor,
            [ArmTypes.DocumentIntelligenceType] = DocumentIntelligence,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps Azure ARM resource type strings to their friendly type names.
    /// Case-insensitive lookup.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ArmTypeToFriendlyName => ArmTypeToFriendlyNameMap;

    /// <summary>
    /// All known friendly type names.
    /// </summary>
    public static readonly IReadOnlyList<string> All =
    [
        KeyVault, RedisCache, StorageAccount, AppServicePlan,
        WebApp, FunctionApp, UserAssignedIdentity, AppConfiguration,
        ContainerAppEnvironment, ContainerApp, LogAnalyticsWorkspace,
        ApplicationInsights, CosmosDb, SqlServer, SqlDatabase,
        ServiceBusNamespace, ContainerRegistry, EventHubNamespace,
        DocumentIntelligence,
    ];

    /// <summary>
    /// ARM resource types that support per-environment app settings or environment variables
    /// (Web Apps, Function Apps, Container Apps).
    /// </summary>
    public static readonly IReadOnlySet<string> ComputeArmTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ArmTypes.WebAppType,
        ArmTypes.FunctionAppType,
        ArmTypes.ContainerAppType,
    };

    /// <summary>
    /// Resolves the friendly type name from an ARM resource type string.
    /// Returns the original string if no mapping is found.
    /// </summary>
    public static string GetFriendlyName(string armResourceType) =>
        ArmTypeToFriendlyName.GetValueOrDefault(armResourceType, armResourceType);
}
