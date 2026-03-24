using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.SqlServerAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="SqlServer"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class SqlServerEnvironmentSettings : Entity<SqlServerEnvironmentSettingsId>
{
    /// <summary>Gets the parent SQL Server identifier.</summary>
    public AzureResourceId SqlServerId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the minimal TLS version override for this environment.</summary>
    public string? MinimalTlsVersion { get; private set; }

    private SqlServerEnvironmentSettings() { }

    internal SqlServerEnvironmentSettings(
        AzureResourceId sqlServerId,
        string environmentName,
        string? minimalTlsVersion)
        : base(SqlServerEnvironmentSettingsId.CreateUnique())
    {
        SqlServerId = sqlServerId;
        EnvironmentName = environmentName;
        MinimalTlsVersion = minimalTlsVersion;
    }

    /// <summary>
    /// Creates a new <see cref="SqlServerEnvironmentSettings"/> for the specified server and environment.
    /// </summary>
    public static SqlServerEnvironmentSettings Create(
        AzureResourceId sqlServerId,
        string environmentName,
        string? minimalTlsVersion)
        => new(sqlServerId, environmentName, minimalTlsVersion);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(string? minimalTlsVersion)
    {
        MinimalTlsVersion = minimalTlsVersion;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (MinimalTlsVersion is not null) dict["minimalTlsVersion"] = MinimalTlsVersion;
        return dict;
    }
}
