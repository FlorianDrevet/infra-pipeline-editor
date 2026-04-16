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

    /// <summary>
    /// Azure ARM resource provider type strings (e.g. "Microsoft.KeyVault/vaults").
    /// </summary>
    public static class ArmTypes
    {
        public const string KeyVault = "Microsoft.KeyVault/vaults";
        public const string RedisCache = "Microsoft.Cache/Redis";
        public const string StorageAccount = "Microsoft.Storage/storageAccounts";
        public const string AppServicePlan = "Microsoft.Web/serverfarms";
        public const string WebApp = "Microsoft.Web/sites";
        public const string FunctionApp = "Microsoft.Web/sites/functionapp";
        public const string UserAssignedIdentity = "Microsoft.ManagedIdentity/userAssignedIdentities";
        public const string AppConfiguration = "Microsoft.AppConfiguration/configurationStores";
        public const string ContainerAppEnvironment = "Microsoft.App/managedEnvironments";
        public const string ContainerApp = "Microsoft.App/containerApps";
        public const string LogAnalyticsWorkspace = "Microsoft.OperationalInsights/workspaces";
        public const string ApplicationInsights = "Microsoft.Insights/components";
        public const string CosmosDb = "Microsoft.DocumentDB/databaseAccounts";
        public const string SqlServer = "Microsoft.Sql/servers";
        public const string SqlDatabase = "Microsoft.Sql/servers/databases";
        public const string ServiceBusNamespace = "Microsoft.ServiceBus/namespaces";
        public const string ContainerRegistry = "Microsoft.ContainerRegistry/registries";
        public const string EventHubNamespace = "Microsoft.EventHub/namespaces";
    }

    /// <summary>
    /// Maps Azure ARM resource type strings to their friendly type names.
    /// Case-insensitive lookup.
    /// </summary>
    public static readonly Dictionary<string, string> ArmTypeToFriendlyName = new(StringComparer.OrdinalIgnoreCase)
    {
        [ArmTypes.KeyVault] = KeyVault,
        [ArmTypes.RedisCache] = RedisCache,
        [ArmTypes.StorageAccount] = StorageAccount,
        [ArmTypes.AppServicePlan] = AppServicePlan,
        [ArmTypes.WebApp] = WebApp,
        [ArmTypes.FunctionApp] = FunctionApp,
        [ArmTypes.UserAssignedIdentity] = UserAssignedIdentity,
        [ArmTypes.AppConfiguration] = AppConfiguration,
        [ArmTypes.ContainerAppEnvironment] = ContainerAppEnvironment,
        [ArmTypes.ContainerApp] = ContainerApp,
        [ArmTypes.LogAnalyticsWorkspace] = LogAnalyticsWorkspace,
        [ArmTypes.ApplicationInsights] = ApplicationInsights,
        [ArmTypes.CosmosDb] = CosmosDb,
        [ArmTypes.SqlServer] = SqlServer,
        [ArmTypes.SqlDatabase] = SqlDatabase,
        [ArmTypes.ServiceBusNamespace] = ServiceBusNamespace,
        [ArmTypes.ContainerRegistry] = ContainerRegistry,
        [ArmTypes.EventHubNamespace] = EventHubNamespace,
    };

    /// <summary>
    /// All known friendly type names.
    /// </summary>
    public static readonly IReadOnlyCollection<string> All =
    [
        KeyVault, RedisCache, StorageAccount, AppServicePlan,
        WebApp, FunctionApp, UserAssignedIdentity, AppConfiguration,
        ContainerAppEnvironment, ContainerApp, LogAnalyticsWorkspace,
        ApplicationInsights, CosmosDb, SqlServer, SqlDatabase,
        ServiceBusNamespace, ContainerRegistry, EventHubNamespace,
    ];

    /// <summary>
    /// Resolves the friendly type name from an ARM resource type string.
    /// Returns the original string if no mapping is found.
    /// </summary>
    public static string GetFriendlyName(string armResourceType) =>
        ArmTypeToFriendlyName.GetValueOrDefault(armResourceType, armResourceType);
}
