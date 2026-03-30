using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.SqlDatabaseAggregate;

/// <summary>Represents an Azure SQL Database resource.</summary>
public class SqlDatabase : AzureResource
{
    private readonly List<SqlDatabaseEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this SQL Database.</summary>
    public IReadOnlyCollection<SqlDatabaseEnvironmentSettings> EnvironmentSettings
        => _environmentSettings.AsReadOnly();

    /// <summary>Gets the identifier of the SQL Server that hosts this database.</summary>
    public AzureResourceId SqlServerId { get; private set; } = null!;

    /// <summary>Gets the collation of the database.</summary>
    public string Collation { get; private set; } = "SQL_Latin1_General_CP1_CI_AS";

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages
        => Array.Empty<ParameterUsage>();

    private SqlDatabase()
    {
    }

    /// <summary>Updates the mutable properties of this SQL Database.</summary>
    public void Update(
        Name name,
        Location location,
        AzureResourceId sqlServerId,
        string collation)
    {
        Name = name;
        Location = location;
        SqlServerId = sqlServerId;
        Collation = collation;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        SqlDatabaseSku? sku,
        int? maxSizeGb,
        bool? zoneRedundant)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, maxSizeGb, zoneRedundant);
        }
        else
        {
            _environmentSettings.Add(
                SqlDatabaseEnvironmentSettings.Create(Id, environmentName, sku, maxSizeGb, zoneRedundant));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, SqlDatabaseSku? Sku, int? MaxSizeGb, bool? ZoneRedundant)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku, maxSizeGb, zoneRedundant) in settings)
        {
            _environmentSettings.Add(
                SqlDatabaseEnvironmentSettings.Create(Id, envName, sku, maxSizeGb, zoneRedundant));
        }
    }

    /// <summary>Creates a new SQL Database with a generated identifier.</summary>
    public static SqlDatabase Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId sqlServerId,
        string collation,
        IReadOnlyList<(string EnvironmentName, SqlDatabaseSku? Sku, int? MaxSizeGb, bool? ZoneRedundant)>? environmentSettings = null)
    {
        var database = new SqlDatabase
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            SqlServerId = sqlServerId,
            Collation = collation
        };

        if (environmentSettings is not null)
            database.SetAllEnvironmentSettings(environmentSettings);

        return database;
    }
}
