using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ContainerAppAggregate;

/// <summary>
/// Represents an Azure Container App resource aggregate root.
/// </summary>
public class ContainerApp : AzureResource
{
    private readonly List<ContainerAppEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Container App.</summary>
    public IReadOnlyCollection<ContainerAppEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the identifier of the Container App Environment that hosts this Container App.</summary>
    public AzureResourceId ContainerAppEnvironmentId { get; private set; } = null!;

    /// <summary>Gets the optional Container Registry identifier for authenticated image pulls.</summary>
    public AzureResourceId? ContainerRegistryId { get; private set; }

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private ContainerApp()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Container App resource.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="location">The new Azure region.</param>
    /// <param name="containerAppEnvironmentId">The identifier of the hosting Container App Environment.</param>
    /// <param name="containerRegistryId">The optional Container Registry identifier for authenticated image pulls.</param>
    public void Update(Name name, Location location, AzureResourceId containerAppEnvironmentId, AzureResourceId? containerRegistryId)
    {
        Name = name;
        Location = location;
        ContainerAppEnvironmentId = containerAppEnvironmentId;
        ContainerRegistryId = containerRegistryId;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        string? containerImage,
        string? cpuCores,
        string? memoryGi,
        int? minReplicas,
        int? maxReplicas,
        bool? ingressEnabled,
        int? ingressTargetPort,
        bool? ingressExternal,
        string? transportMethod)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(containerImage, cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, ingressTargetPort, ingressExternal, transportMethod);
        }
        else
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentSettings.Create(
                    Id, environmentName, containerImage, cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, ingressTargetPort, ingressExternal, transportMethod));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, string? ContainerImage, string? CpuCores, string? MemoryGi, int? MinReplicas, int? MaxReplicas, bool? IngressEnabled, int? IngressTargetPort, bool? IngressExternal, string? TransportMethod)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentSettings.Create(
                    Id, s.EnvironmentName, s.ContainerImage, s.CpuCores, s.MemoryGi, s.MinReplicas, s.MaxReplicas, s.IngressEnabled, s.IngressTargetPort, s.IngressExternal, s.TransportMethod));
        }
    }

    /// <summary>
    /// Creates a new <see cref="ContainerApp"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="containerAppEnvironmentId">The identifier of the hosting Container App Environment.</param>
    /// <param name="containerRegistryId">The optional Container Registry identifier for authenticated image pulls.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <returns>A new <see cref="ContainerApp"/> aggregate root.</returns>
    public static ContainerApp Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId containerAppEnvironmentId,
        AzureResourceId? containerRegistryId,
        IReadOnlyList<(string EnvironmentName, string? ContainerImage, string? CpuCores, string? MemoryGi, int? MinReplicas, int? MaxReplicas, bool? IngressEnabled, int? IngressTargetPort, bool? IngressExternal, string? TransportMethod)>? environmentSettings = null)
    {
        var containerApp = new ContainerApp
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            ContainerAppEnvironmentId = containerAppEnvironmentId,
            ContainerRegistryId = containerRegistryId
        };

        if (environmentSettings is not null)
            containerApp.SetAllEnvironmentSettings(environmentSettings);

        return containerApp;
    }
}
