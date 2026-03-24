using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.SqlDatabases.Requests;

/// <summary>Common properties shared by create and update SQL Database requests.</summary>
public abstract class SqlDatabaseRequestBase
{
    /// <summary>Display name for the SQL Database resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the SQL Database will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Identifier of the SQL Server that hosts this database.</summary>
    [Required, GuidValidation]
    public required Guid SqlServerId { get; init; }

    /// <summary>Database collation (e.g., "SQL_Latin1_General_CP1_CI_AS").</summary>
    [Required]
    public required string Collation { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<SqlDatabaseEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a SQL Database.</summary>
public class SqlDatabaseEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional SKU tier override (e.g., "Basic", "Standard").</summary>
    [EnumValidation(typeof(SqlDatabaseSku.SqlDatabaseSkuEnum))]
    public string? Sku { get; init; }

    /// <summary>Optional maximum database size in GB.</summary>
    public int? MaxSizeGb { get; init; }

    /// <summary>Optional zone redundancy override.</summary>
    public bool? ZoneRedundant { get; init; }
}

/// <summary>Response DTO for a typed per-environment SQL Database configuration.</summary>
public record SqlDatabaseEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? MaxSizeGb,
    bool? ZoneRedundant);
