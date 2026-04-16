using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ContainerRegistryAggregate;

/// <summary>
/// Represents an Azure Container Registry resource aggregate root.
/// </summary>
public sealed class ContainerRegistry : AzureResource
{
    private readonly List<ContainerRegistryEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Container Registry.</summary>
    public IReadOnlyCollection<ContainerRegistryEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private ContainerRegistry()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Container Registry resource.
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
        string? sku,
        bool? adminUserEnabled,
        string? publicNetworkAccess,
        bool? zoneRedundancy)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, adminUserEnabled, publicNetworkAccess, zoneRedundancy);
        }
        else
        {
            _environmentSettings.Add(
                ContainerRegistryEnvironmentSettings.Create(
                    Id, environmentName, sku, adminUserEnabled, publicNetworkAccess, zoneRedundancy));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyCollection<(string EnvironmentName, string? Sku, bool? AdminUserEnabled, string? PublicNetworkAccess, bool? ZoneRedundancy)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku, adminUserEnabled, publicNetworkAccess, zoneRedundancy) in settings)
        {
            _environmentSettings.Add(
                ContainerRegistryEnvironmentSettings.Create(
                    Id, envName, sku, adminUserEnabled, publicNetworkAccess, zoneRedundancy));
        }
    }

    /// <summary>
    /// Creates a new <see cref="ContainerRegistry"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <returns>A new <see cref="ContainerRegistry"/> aggregate root.</returns>
    public static ContainerRegistry Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyCollection<(string EnvironmentName, string? Sku, bool? AdminUserEnabled, string? PublicNetworkAccess, bool? ZoneRedundancy)>? environmentSettings = null)
    {
        var containerRegistry = new ContainerRegistry
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            containerRegistry.SetAllEnvironmentSettings(environmentSettings);

        return containerRegistry;
    }
}
