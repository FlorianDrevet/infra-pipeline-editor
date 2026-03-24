namespace InfraFlowSculptor.Application.CosmosDbs.Common;

/// <summary>
/// Carries typed per-environment Cosmos DB data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="DatabaseApiType">Optional database API type (e.g., "SQL", "MongoDB", "Cassandra", "Table", "Gremlin").</param>
/// <param name="ConsistencyLevel">Optional consistency level (e.g., "Eventual", "Session", "Strong").</param>
/// <param name="MaxStalenessPrefix">Optional maximum staleness prefix for BoundedStaleness consistency.</param>
/// <param name="MaxIntervalInSeconds">Optional maximum interval in seconds for BoundedStaleness consistency.</param>
/// <param name="EnableAutomaticFailover">Optional flag to enable automatic failover.</param>
/// <param name="EnableMultipleWriteLocations">Optional flag to enable multi-region writes.</param>
/// <param name="BackupPolicyType">Optional backup policy type (e.g., "Periodic", "Continuous").</param>
/// <param name="EnableFreeTier">Optional flag to apply the free tier discount.</param>
public record CosmosDbEnvironmentConfigData(
    string EnvironmentName,
    string? DatabaseApiType,
    string? ConsistencyLevel,
    int? MaxStalenessPrefix,
    int? MaxIntervalInSeconds,
    bool? EnableAutomaticFailover,
    bool? EnableMultipleWriteLocations,
    string? BackupPolicyType,
    bool? EnableFreeTier);
