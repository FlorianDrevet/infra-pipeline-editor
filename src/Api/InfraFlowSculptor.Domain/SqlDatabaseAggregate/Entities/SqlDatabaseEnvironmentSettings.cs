using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="SqlDatabase"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class SqlDatabaseEnvironmentSettings : Entity<SqlDatabaseEnvironmentSettingsId>
{
    /// <summary>Gets the parent SQL Database identifier.</summary>
    public AzureResourceId SqlDatabaseId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the SKU tier override for this environment.</summary>
    public SqlDatabaseSku? Sku { get; private set; }

    /// <summary>Gets or sets the maximum size in GB for this environment.</summary>
    public int? MaxSizeGb { get; private set; }

    /// <summary>Gets or sets whether zone redundancy is enabled for this environment.</summary>
    public bool? ZoneRedundant { get; private set; }

    private SqlDatabaseEnvironmentSettings() { }

    internal SqlDatabaseEnvironmentSettings(
        AzureResourceId sqlDatabaseId,
        string environmentName,
        SqlDatabaseSku? sku,
        int? maxSizeGb,
        bool? zoneRedundant)
        : base(SqlDatabaseEnvironmentSettingsId.CreateUnique())
    {
        SqlDatabaseId = sqlDatabaseId;
        EnvironmentName = environmentName;
        Sku = sku;
        MaxSizeGb = maxSizeGb;
        ZoneRedundant = zoneRedundant;
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseEnvironmentSettings"/> for the specified database and environment.
    /// </summary>
    public static SqlDatabaseEnvironmentSettings Create(
        AzureResourceId sqlDatabaseId,
        string environmentName,
        SqlDatabaseSku? sku,
        int? maxSizeGb,
        bool? zoneRedundant)
        => new(sqlDatabaseId, environmentName, sku, maxSizeGb, zoneRedundant);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(SqlDatabaseSku? sku, int? maxSizeGb, bool? zoneRedundant)
    {
        Sku = sku;
        MaxSizeGb = maxSizeGb;
        ZoneRedundant = zoneRedundant;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku.Value.ToString();
        if (MaxSizeGb is not null) dict["maxSizeGb"] = MaxSizeGb.Value.ToString();
        if (ZoneRedundant is not null) dict["zoneRedundant"] = ZoneRedundant.Value.ToString().ToLower();
        return dict;
    }
}
