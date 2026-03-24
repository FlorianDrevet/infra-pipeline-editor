using InfraFlowSculptor.Domain.CosmosDbAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="CosmosDb"/> account.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class CosmosDbEnvironmentSettings : Entity<CosmosDbEnvironmentSettingsId>
{
    /// <summary>Gets the parent Cosmos DB account identifier.</summary>
    public AzureResourceId CosmosDbId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the database API type (e.g., "SQL", "MongoDB", "Cassandra", "Table", "Gremlin").</summary>
    public string? DatabaseApiType { get; private set; }

    /// <summary>Gets or sets the consistency level (e.g., "Eventual", "ConsistentPrefix", "Session", "BoundedStaleness", "Strong").</summary>
    public string? ConsistencyLevel { get; private set; }

    /// <summary>Gets or sets the maximum staleness prefix. Only meaningful when ConsistencyLevel is BoundedStaleness.</summary>
    public int? MaxStalenessPrefix { get; private set; }

    /// <summary>Gets or sets the maximum interval in seconds. Only meaningful when ConsistencyLevel is BoundedStaleness.</summary>
    public int? MaxIntervalInSeconds { get; private set; }

    /// <summary>Gets or sets whether automatic failover is enabled.</summary>
    public bool? EnableAutomaticFailover { get; private set; }

    /// <summary>Gets or sets whether multi-region writes are enabled.</summary>
    public bool? EnableMultipleWriteLocations { get; private set; }

    /// <summary>Gets or sets the backup policy type (e.g., "Periodic", "Continuous").</summary>
    public string? BackupPolicyType { get; private set; }

    /// <summary>Gets or sets whether the free tier discount is applied.</summary>
    public bool? EnableFreeTier { get; private set; }

    private CosmosDbEnvironmentSettings() { }

    internal CosmosDbEnvironmentSettings(
        AzureResourceId cosmosDbId,
        string environmentName,
        string? databaseApiType,
        string? consistencyLevel,
        int? maxStalenessPrefix,
        int? maxIntervalInSeconds,
        bool? enableAutomaticFailover,
        bool? enableMultipleWriteLocations,
        string? backupPolicyType,
        bool? enableFreeTier)
        : base(CosmosDbEnvironmentSettingsId.CreateUnique())
    {
        CosmosDbId = cosmosDbId;
        EnvironmentName = environmentName;
        DatabaseApiType = databaseApiType;
        ConsistencyLevel = consistencyLevel;
        MaxStalenessPrefix = maxStalenessPrefix;
        MaxIntervalInSeconds = maxIntervalInSeconds;
        EnableAutomaticFailover = enableAutomaticFailover;
        EnableMultipleWriteLocations = enableMultipleWriteLocations;
        BackupPolicyType = backupPolicyType;
        EnableFreeTier = enableFreeTier;
    }

    /// <summary>
    /// Creates a new <see cref="CosmosDbEnvironmentSettings"/> for the specified Cosmos DB account and environment.
    /// </summary>
    public static CosmosDbEnvironmentSettings Create(
        AzureResourceId cosmosDbId,
        string environmentName,
        string? databaseApiType,
        string? consistencyLevel,
        int? maxStalenessPrefix,
        int? maxIntervalInSeconds,
        bool? enableAutomaticFailover,
        bool? enableMultipleWriteLocations,
        string? backupPolicyType,
        bool? enableFreeTier)
        => new(cosmosDbId, environmentName, databaseApiType, consistencyLevel, maxStalenessPrefix, maxIntervalInSeconds, enableAutomaticFailover, enableMultipleWriteLocations, backupPolicyType, enableFreeTier);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        string? databaseApiType,
        string? consistencyLevel,
        int? maxStalenessPrefix,
        int? maxIntervalInSeconds,
        bool? enableAutomaticFailover,
        bool? enableMultipleWriteLocations,
        string? backupPolicyType,
        bool? enableFreeTier)
    {
        DatabaseApiType = databaseApiType;
        ConsistencyLevel = consistencyLevel;
        MaxStalenessPrefix = maxStalenessPrefix;
        MaxIntervalInSeconds = maxIntervalInSeconds;
        EnableAutomaticFailover = enableAutomaticFailover;
        EnableMultipleWriteLocations = enableMultipleWriteLocations;
        BackupPolicyType = backupPolicyType;
        EnableFreeTier = enableFreeTier;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (DatabaseApiType is not null) dict["databaseApiType"] = DatabaseApiType;
        if (ConsistencyLevel is not null) dict["consistencyLevel"] = ConsistencyLevel;
        if (MaxStalenessPrefix is not null) dict["maxStalenessPrefix"] = MaxStalenessPrefix.Value.ToString();
        if (MaxIntervalInSeconds is not null) dict["maxIntervalInSeconds"] = MaxIntervalInSeconds.Value.ToString();
        if (EnableAutomaticFailover is not null) dict["enableAutomaticFailover"] = EnableAutomaticFailover.Value.ToString().ToLower();
        if (EnableMultipleWriteLocations is not null) dict["enableMultipleWriteLocations"] = EnableMultipleWriteLocations.Value.ToString().ToLower();
        if (BackupPolicyType is not null) dict["backupPolicyType"] = BackupPolicyType;
        if (EnableFreeTier is not null) dict["enableFreeTier"] = EnableFreeTier.Value.ToString().ToLower();
        return dict;
    }
}
