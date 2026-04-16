using InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.CosmosDbAggregate;

/// <summary>
/// Represents an Azure Cosmos DB database account resource aggregate root.
/// </summary>
public sealed class CosmosDb : AzureResource
{
    private readonly List<CosmosDbEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Cosmos DB account.</summary>
    public IReadOnlyCollection<CosmosDbEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private CosmosDb()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Cosmos DB resource.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="location">The new Azure region.</param>
    public void Update(Name name, Location location)
    {
        Name = name;
        Location = location;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        string? databaseApiType,
        string? consistencyLevel,
        int? maxStalenessPrefix,
        int? maxIntervalInSeconds,
        bool? enableAutomaticFailover,
        bool? enableMultipleWriteLocations,
        string? backupPolicyType,
        bool? enableFreeTier)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(databaseApiType, consistencyLevel, maxStalenessPrefix, maxIntervalInSeconds, enableAutomaticFailover, enableMultipleWriteLocations, backupPolicyType, enableFreeTier);
        }
        else
        {
            _environmentSettings.Add(
                CosmosDbEnvironmentSettings.Create(
                    Id, environmentName, databaseApiType, consistencyLevel, maxStalenessPrefix, maxIntervalInSeconds, enableAutomaticFailover, enableMultipleWriteLocations, backupPolicyType, enableFreeTier));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyCollection<(string EnvironmentName, string? DatabaseApiType, string? ConsistencyLevel, int? MaxStalenessPrefix, int? MaxIntervalInSeconds, bool? EnableAutomaticFailover, bool? EnableMultipleWriteLocations, string? BackupPolicyType, bool? EnableFreeTier)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, databaseApiType, consistencyLevel, maxStalenessPrefix, maxIntervalInSeconds, enableAutomaticFailover, enableMultipleWriteLocations, backupPolicyType, enableFreeTier) in settings)
        {
            _environmentSettings.Add(
                CosmosDbEnvironmentSettings.Create(
                    Id, envName, databaseApiType, consistencyLevel, maxStalenessPrefix, maxIntervalInSeconds, enableAutomaticFailover, enableMultipleWriteLocations, backupPolicyType, enableFreeTier));
        }
    }

    /// <summary>
    /// Creates a new <see cref="CosmosDb"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <returns>A new <see cref="CosmosDb"/> aggregate root.</returns>
    public static CosmosDb Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyCollection<(string EnvironmentName, string? DatabaseApiType, string? ConsistencyLevel, int? MaxStalenessPrefix, int? MaxIntervalInSeconds, bool? EnableAutomaticFailover, bool? EnableMultipleWriteLocations, string? BackupPolicyType, bool? EnableFreeTier)>? environmentSettings = null)
    {
        var cosmosDb = new CosmosDb
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            cosmosDb.SetAllEnvironmentSettings(environmentSettings);

        return cosmosDb;
    }
}
