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
public sealed class ContainerApp : AzureResource
{
    private readonly List<ContainerAppEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Container App.</summary>
    public IReadOnlyCollection<ContainerAppEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the identifier of the Container App Environment that hosts this Container App.</summary>
    public AzureResourceId ContainerAppEnvironmentId { get; private set; } = null!;

    /// <summary>Gets the optional Container Registry identifier for authenticated image pulls.</summary>
    public AzureResourceId? ContainerRegistryId { get; private set; }

    /// <summary>Gets the optional base Docker image name (e.g., "myregistry.azurecr.io/myapp/api") without the tag.</summary>
    public string? DockerImageName { get; private set; }

    /// <summary>Gets the optional relative path to the Dockerfile in the repository.</summary>
    public string? DockerfilePath { get; private set; }

    /// <summary>Gets the user-friendly application name displayed in Azure DevOps pipeline runs.</summary>
    public string? ApplicationName { get; private set; }

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
    /// <param name="dockerImageName">The optional base Docker image name without the tag.</param>
    /// <param name="dockerfilePath">The optional relative path to the Dockerfile in the repository.</param>
    /// <param name="applicationName">The optional user-friendly application name for pipeline display.</param>
    public void Update(Name name, Location location, AzureResourceId containerAppEnvironmentId, AzureResourceId? containerRegistryId, string? dockerImageName, string? dockerfilePath, string? applicationName)
    {
        Name = name;
        Location = location;

        if (IsExisting)
            return;

        ContainerAppEnvironmentId = containerAppEnvironmentId;
        ContainerRegistryId = containerRegistryId;
        DockerImageName = dockerImageName;
        DockerfilePath = dockerfilePath;
        ApplicationName = applicationName;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetEnvironmentSettings(
        string environmentName,
        string? cpuCores,
        string? memoryGi,
        int? minReplicas,
        int? maxReplicas,
        bool? ingressEnabled,
        int? ingressTargetPort,
        bool? ingressExternal,
        string? transportMethod,
        string? readinessProbePath = null,
        int? readinessProbePort = null,
        string? livenessProbePath = null,
        int? livenessProbePort = null,
        string? startupProbePath = null,
        int? startupProbePort = null)
    {
        if (IsExisting)
            return;
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, ingressTargetPort, ingressExternal, transportMethod, readinessProbePath, readinessProbePort, livenessProbePath, livenessProbePort, startupProbePath, startupProbePort);
        }
        else
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentSettings.Create(
                    Id, environmentName, cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, ingressTargetPort, ingressExternal, transportMethod, readinessProbePath, readinessProbePort, livenessProbePath, livenessProbePort, startupProbePath, startupProbePort));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, string? CpuCores, string? MemoryGi, int? MinReplicas, int? MaxReplicas, bool? IngressEnabled, int? IngressTargetPort, bool? IngressExternal, string? TransportMethod, string? ReadinessProbePath, int? ReadinessProbePort, string? LivenessProbePath, int? LivenessProbePort, string? StartupProbePath, int? StartupProbePort)> settings)
    {
        if (IsExisting)
            return;

        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentSettings.Create(
                    Id, s.EnvironmentName, s.CpuCores, s.MemoryGi, s.MinReplicas, s.MaxReplicas, s.IngressEnabled, s.IngressTargetPort, s.IngressExternal, s.TransportMethod, s.ReadinessProbePath, s.ReadinessProbePort, s.LivenessProbePath, s.LivenessProbePort, s.StartupProbePath, s.StartupProbePort));
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
    /// <param name="dockerImageName">The optional base Docker image name without the tag.</param>
    /// <param name="dockerfilePath">The optional relative path to the Dockerfile in the repository.</param>
    /// <param name="applicationName">The optional user-friendly application name for pipeline display.</param>
    /// <summary>
    /// Creates a new <see cref="ContainerApp"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="containerAppEnvironmentId">The identifier of the hosting Container App Environment.</param>
    /// <param name="containerRegistryId">The optional Container Registry identifier for authenticated image pulls.</param>
    /// <param name="dockerImageName">The optional base Docker image name without the tag.</param>
    /// <param name="dockerfilePath">The optional relative path to the Dockerfile in the repository.</param>
    /// <param name="applicationName">The optional user-friendly application name for pipeline display.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <param name="isExisting">When <c>true</c>, this resource already exists in Azure and is not deployed by this project.</param>
    public static ContainerApp Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId containerAppEnvironmentId,
        AzureResourceId? containerRegistryId,
        string? dockerImageName = null,
        string? dockerfilePath = null,
        string? applicationName = null,
        IReadOnlyList<(string EnvironmentName, string? CpuCores, string? MemoryGi, int? MinReplicas, int? MaxReplicas, bool? IngressEnabled, int? IngressTargetPort, bool? IngressExternal, string? TransportMethod, string? ReadinessProbePath, int? ReadinessProbePort, string? LivenessProbePath, int? LivenessProbePort, string? StartupProbePath, int? StartupProbePort)>? environmentSettings = null,
        bool isExisting = false)
    {
        var containerApp = new ContainerApp
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting,
            ContainerAppEnvironmentId = containerAppEnvironmentId,
            ContainerRegistryId = containerRegistryId,
            DockerImageName = dockerImageName,
            DockerfilePath = dockerfilePath,
            ApplicationName = applicationName
        };

        if (!isExisting && environmentSettings is not null)
            containerApp.SetAllEnvironmentSettings(environmentSettings);

        return containerApp;
    }
}
