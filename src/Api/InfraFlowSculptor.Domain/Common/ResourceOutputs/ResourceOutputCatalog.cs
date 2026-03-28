namespace InfraFlowSculptor.Domain.Common.ResourceOutputs;

/// <summary>
/// Describes a single output that a resource type exposes and that can be
/// referenced as an environment variable / app setting in a compute resource.
/// </summary>
/// <param name="Name">The Bicep output name (e.g., "vaultUri").</param>
/// <param name="Description">Human-readable description of the output.</param>
/// <param name="BicepExpression">The Bicep property expression used in the module (e.g., "kv.properties.vaultUri").</param>
/// <param name="IsSensitive">Whether this output contains sensitive data (keys, connection strings, passwords).</param>
public sealed record ResourceOutputDefinition(string Name, string Description, string BicepExpression, bool IsSensitive = false);

/// <summary>
/// Catalog of available outputs per Azure resource type.
/// Used by the frontend to display selectable outputs when configuring app settings,
/// and by the Bicep generator to emit the correct <c>output</c> declarations.
/// </summary>
public static class ResourceOutputCatalog
{
    private static readonly Dictionary<string, IReadOnlyList<ResourceOutputDefinition>> Outputs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KeyVault"] =
        [
            new("vaultUri", "Key Vault URI", "kv.properties.vaultUri"),
            new("vaultName", "Key Vault name", "kv.name"),
        ],
        ["RedisCache"] =
        [
            new("hostName", "Redis host name", "redisCache.properties.hostName"),
            new("sslPort", "Redis SSL port", "string(redisCache.properties.sslPort)"),
            new("connectionString", "Redis connection string", "'${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False'", IsSensitive: true),
            new("primaryKey", "Redis primary access key", "redisCache.listKeys().primaryKey", IsSensitive: true),
        ],
        ["StorageAccount"] =
        [
            new("storageAccountName", "Storage account name", "storageAccount.name"),
            new("primaryBlobEndpoint", "Primary Blob endpoint", "storageAccount.properties.primaryEndpoints.blob"),
            new("primaryQueueEndpoint", "Primary Queue endpoint", "storageAccount.properties.primaryEndpoints.queue"),
            new("primaryTableEndpoint", "Primary Table endpoint", "storageAccount.properties.primaryEndpoints.table"),
            new("connectionString", "Storage account connection string", "'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value}'", IsSensitive: true),
            new("primaryKey", "Storage account primary access key", "storageAccount.listKeys().keys[0].value", IsSensitive: true),
        ],
        ["AppConfiguration"] =
        [
            new("endpoint", "App Configuration endpoint", "appConfiguration.properties.endpoint"),
        ],
        ["ApplicationInsights"] =
        [
            new("connectionString", "Application Insights connection string", "appInsights.properties.ConnectionString"),
            new("instrumentationKey", "Application Insights instrumentation key", "appInsights.properties.InstrumentationKey"),
        ],
        ["LogAnalyticsWorkspace"] =
        [
            new("workspaceId", "Log Analytics workspace ID", "logAnalyticsWorkspace.properties.customerId"),
        ],
        ["CosmosDb"] =
        [
            new("documentEndpoint", "Cosmos DB document endpoint", "cosmosDb.properties.documentEndpoint"),
            new("primaryConnectionString", "Cosmos DB primary connection string", "cosmosDb.listConnectionStrings().connectionStrings[0].connectionString", IsSensitive: true),
            new("primaryKey", "Cosmos DB primary master key", "cosmosDb.listKeys().primaryMasterKey", IsSensitive: true),
        ],
        ["SqlServer"] =
        [
            new("fullyQualifiedDomainName", "SQL Server FQDN", "sqlServer.properties.fullyQualifiedDomainName"),
            new("connectionString", "SQL Server connection string (ADO.NET)", "'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Authentication=Active Directory Default;'", IsSensitive: true),
        ],
        ["ContainerAppEnvironment"] =
        [
            new("defaultDomain", "Container App Environment default domain", "containerAppEnvironment.properties.defaultDomain"),
            new("staticIp", "Container App Environment static IP", "containerAppEnvironment.properties.staticIp"),
        ],
        ["UserAssignedIdentity"] =
        [
            new("principalId", "Managed identity principal ID", "identity.properties.principalId"),
            new("clientId", "Managed identity client ID", "identity.properties.clientId"),
        ],
        ["AppServicePlan"] =
        [
            new("id", "App Service Plan resource ID", "asp.id"),
        ],
        ["WebApp"] =
        [
            new("defaultHostName", "Web App default host name", "webApp.properties.defaultHostName"),
        ],
        ["FunctionApp"] =
        [
            new("defaultHostName", "Function App default host name", "functionApp.properties.defaultHostName"),
        ],
        ["ContainerApp"] =
        [
            new("fqdn", "Container App FQDN", "containerApp.properties.configuration.ingress.fqdn"),
        ],
        ["SqlDatabase"] =
        [
            new("id", "SQL Database resource ID", "sqlDatabase.id"),
        ],
        ["ServiceBusNamespace"] =
        [
            new("connectionString", "Service Bus primary connection string", "serviceBusNamespace.listKeys('${serviceBusNamespace.id}/authorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString", IsSensitive: true),
        ],
    };

    /// <summary>
    /// Returns the available outputs for the given resource type name.
    /// </summary>
    public static IReadOnlyList<ResourceOutputDefinition> GetForResourceType(string resourceTypeName)
        => Outputs.GetValueOrDefault(resourceTypeName) ?? [];

    /// <summary>
    /// Returns all resource types that have at least one output defined.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<ResourceOutputDefinition>> GetAll() => Outputs;

    /// <summary>
    /// Finds a specific output definition by resource type name and output name.
    /// </summary>
    public static ResourceOutputDefinition? FindOutput(string resourceTypeName, string outputName)
        => GetForResourceType(resourceTypeName).FirstOrDefault(o =>
            o.Name.Equals(outputName, StringComparison.OrdinalIgnoreCase));
}
