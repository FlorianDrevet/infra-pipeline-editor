using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate.Entities;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.SqlServerAggregate;

/// <summary>Represents an Azure SQL Server resource.</summary>
public class SqlServer : AzureResource
{
    private readonly List<SqlServerEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this SQL Server.</summary>
    public IReadOnlyCollection<SqlServerEnvironmentSettings> EnvironmentSettings
        => _environmentSettings.AsReadOnly();

    /// <summary>Gets the SQL Server version (e.g., V12).</summary>
    public SqlServerVersion Version { get; private set; } = null!;

    /// <summary>Gets the administrator login name for the SQL Server.</summary>
    public string AdministratorLogin { get; private set; } = string.Empty;

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages
        => Array.Empty<ParameterUsage>();

    private SqlServer()
    {
    }

    /// <summary>Updates the mutable properties of this SQL Server.</summary>
    public void Update(Name name, Location location, SqlServerVersion version, string administratorLogin)
    {
        Name = name;
        Location = location;
        Version = version;
        AdministratorLogin = administratorLogin;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        string? minimalTlsVersion)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(minimalTlsVersion);
        }
        else
        {
            _environmentSettings.Add(
                SqlServerEnvironmentSettings.Create(Id, environmentName, minimalTlsVersion));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, string? MinimalTlsVersion)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, minTls) in settings)
        {
            _environmentSettings.Add(
                SqlServerEnvironmentSettings.Create(Id, envName, minTls));
        }
    }

    /// <summary>Creates a new SQL Server with a generated identifier.</summary>
    public static SqlServer Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        SqlServerVersion version,
        string administratorLogin,
        IReadOnlyList<(string EnvironmentName, string? MinimalTlsVersion)>? environmentSettings = null)
    {
        var server = new SqlServer
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            Version = version,
            AdministratorLogin = administratorLogin
        };

        if (environmentSettings is not null)
            server.SetAllEnvironmentSettings(environmentSettings);

        return server;
    }
}
