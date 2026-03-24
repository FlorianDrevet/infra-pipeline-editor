using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.CosmosDbs.Requests;

/// <summary>Common properties shared by create and update Cosmos DB requests.</summary>
public abstract class CosmosDbRequestBase
{
    /// <summary>Display name for the Cosmos DB account resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Cosmos DB account will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<CosmosDbEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Cosmos DB account.</summary>
public class CosmosDbEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional database API type (e.g., "SQL", "MongoDB", "Cassandra", "Table", "Gremlin").</summary>
    public string? DatabaseApiType { get; init; }

    /// <summary>Optional consistency level (e.g., "Eventual", "ConsistentPrefix", "Session", "BoundedStaleness", "Strong").</summary>
    public string? ConsistencyLevel { get; init; }

    /// <summary>Optional maximum staleness prefix for BoundedStaleness consistency (10..2147483647).</summary>
    public int? MaxStalenessPrefix { get; init; }

    /// <summary>Optional maximum interval in seconds for BoundedStaleness consistency (5..86400).</summary>
    public int? MaxIntervalInSeconds { get; init; }

    /// <summary>Optional flag to enable automatic failover.</summary>
    public bool? EnableAutomaticFailover { get; init; }

    /// <summary>Optional flag to enable multi-region writes.</summary>
    public bool? EnableMultipleWriteLocations { get; init; }

    /// <summary>Optional backup policy type (e.g., "Periodic", "Continuous").</summary>
    public string? BackupPolicyType { get; init; }

    /// <summary>Optional flag to apply the free tier discount.</summary>
    public bool? EnableFreeTier { get; init; }
}

/// <summary>Response DTO for a typed per-environment Cosmos DB configuration.</summary>
public record CosmosDbEnvironmentConfigResponse(
    string EnvironmentName,
    string? DatabaseApiType,
    string? ConsistencyLevel,
    int? MaxStalenessPrefix,
    int? MaxIntervalInSeconds,
    bool? EnableAutomaticFailover,
    bool? EnableMultipleWriteLocations,
    string? BackupPolicyType,
    bool? EnableFreeTier);
