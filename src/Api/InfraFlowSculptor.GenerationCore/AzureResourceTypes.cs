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
    /// Centralized ARM API version constants used in Bicep generation.
    /// Updating a version here propagates to all generators, assemblers, and templates.
    /// </summary>
    public static class ApiVersions
    {
        public const string KeyVault = "2023-07-01";
        public const string RedisCache = "2023-08-01";
        public const string StorageAccount = "2025-06-01";
        public const string AppServicePlan = "2023-12-01";
        public const string WebApp = "2023-12-01";
        public const string FunctionApp = "2023-12-01";
        public const string UserAssignedIdentity = "2023-01-31";
        public const string AppConfiguration = "2023-03-01";
        public const string ContainerAppEnvironment = "2024-03-01";
        public const string ContainerApp = "2024-03-01";
        public const string LogAnalyticsWorkspace = "2023-09-01";
        public const string ApplicationInsights = "2020-02-02";
        public const string CosmosDb = "2024-05-15";
        public const string SqlServer = "2023-08-01-preview";
        public const string SqlDatabase = "2023-08-01-preview";
        public const string ServiceBusNamespace = "2022-10-01-preview";
        public const string ContainerRegistry = "2023-07-01";
        public const string EventHubNamespace = "2024-01-01";
        public const string ResourceGroup = "2024-07-01";
        public const string RoleAssignment = "2022-04-01";

        /// <summary>Default fallback version for unregistered resource types.</summary>
        public const string Default = "2023-01-01";

        /// <summary>
        /// Overridden API versions for <c>existing</c> resource references
        /// used in cross-configuration scenarios.
        /// </summary>
        public static class ExistingRef
        {
            public const string RedisCache = "2024-03-01";
            public const string StorageAccount = "2023-05-01";
        }

        /// <summary>
        /// Overridden API versions for <c>existing</c> resource references
        /// used specifically in role assignment modules.
        /// </summary>
        public static class RoleAssignmentRef
        {
            public const string StorageAccount = "2023-01-01";
        }
    }

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
    public static readonly IReadOnlyList<string> All =
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
