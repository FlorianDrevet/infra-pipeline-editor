using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.SqlServers.Requests;

/// <summary>Common properties shared by create and update SQL Server requests.</summary>
public abstract class SqlServerRequestBase
{
    /// <summary>Display name for the SQL Server resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the SQL Server will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>SQL Server version (e.g., "V12").</summary>
    [Required, EnumValidation(typeof(SqlServerVersion.SqlServerVersionEnum))]
    public required string Version { get; init; }

    /// <summary>Administrator login name for the SQL Server.</summary>
    [Required]
    public required string AdministratorLogin { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<SqlServerEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a SQL Server.</summary>
public class SqlServerEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional minimal TLS version override (e.g., "1.0", "1.1", "1.2").</summary>
    public string? MinimalTlsVersion { get; init; }
}

/// <summary>Response DTO for a typed per-environment SQL Server configuration.</summary>
public record SqlServerEnvironmentConfigResponse(
    string EnvironmentName,
    string? MinimalTlsVersion);
