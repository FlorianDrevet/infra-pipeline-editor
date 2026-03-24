using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;

/// <summary>
/// Represents an Azure Container App Environment resource aggregate root.
/// </summary>
public class ContainerAppEnvironment : AzureResource
{
    private readonly List<ContainerAppEnvironmentEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this Container App Environment.</summary>
    public IReadOnlyCollection<ContainerAppEnvironmentEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private ContainerAppEnvironment()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Container App Environment resource.
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
        string? workloadProfileType,
        bool? internalLoadBalancerEnabled,
        bool? zoneRedundancyEnabled,
        string? logAnalyticsWorkspaceId)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, workloadProfileType, internalLoadBalancerEnabled, zoneRedundancyEnabled, logAnalyticsWorkspaceId);
        }
        else
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentEnvironmentSettings.Create(
                    Id, environmentName, sku, workloadProfileType, internalLoadBalancerEnabled, zoneRedundancyEnabled, logAnalyticsWorkspaceId));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, string? Sku, string? WorkloadProfileType, bool? InternalLoadBalancerEnabled, bool? ZoneRedundancyEnabled, string? LogAnalyticsWorkspaceId)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku, workloadProfileType, internalLoadBalancerEnabled, zoneRedundancyEnabled, logAnalyticsWorkspaceId) in settings)
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentEnvironmentSettings.Create(
                    Id, envName, sku, workloadProfileType, internalLoadBalancerEnabled, zoneRedundancyEnabled, logAnalyticsWorkspaceId));
        }
    }

    /// <summary>
    /// Creates a new <see cref="ContainerAppEnvironment"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <returns>A new <see cref="ContainerAppEnvironment"/> aggregate root.</returns>
    public static ContainerAppEnvironment Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyList<(string EnvironmentName, string? Sku, string? WorkloadProfileType, bool? InternalLoadBalancerEnabled, bool? ZoneRedundancyEnabled, string? LogAnalyticsWorkspaceId)>? environmentSettings = null)
    {
        var containerAppEnvironment = new ContainerAppEnvironment
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            containerAppEnvironment.SetAllEnvironmentSettings(environmentSettings);

        return containerAppEnvironment;
    }
}
