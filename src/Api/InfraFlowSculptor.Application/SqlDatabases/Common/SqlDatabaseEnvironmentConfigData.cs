namespace InfraFlowSculptor.Application.SqlDatabases.Common;

/// <summary>
/// Carries typed per-environment SQL Database configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="Sku">Optional SKU tier override (e.g., "Basic", "Standard", "Premium").</param>
/// <param name="MaxSizeGb">Optional maximum database size in GB.</param>
/// <param name="ZoneRedundant">Optional zone redundancy override.</param>
public record SqlDatabaseEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    int? MaxSizeGb,
    bool? ZoneRedundant);
