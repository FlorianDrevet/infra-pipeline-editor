using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Models;
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

    /// <summary>Gets the optional authentication mode used to pull images from Azure Container Registry.</summary>
    public AcrAuthMode? AcrAuthMode { get; private set; }

    /// <summary>Gets the optional User Assigned Identity used exclusively for pulling images from ACR (distinct from the resource-level identity).</summary>
    public AzureResourceId? AcrPullIdentityId { get; private set; }

    /// <summary>Gets the optional base Docker image name (e.g., "myregistry.azurecr.io/myapp/api") without the tag.</summary>
    public string? DockerImageName { get; private set; }

    /// <summary>Gets whether the user has confirmed that the Docker image exists in the container registry.</summary>
    public bool DockerImageValidated { get; private set; }

    /// <summary>Gets the optional relative path to the Dockerfile in the repository.</summary>
    public string? DockerfilePath { get; private set; }

    /// <summary>Gets the user-friendly application name displayed in Azure DevOps pipeline runs.</summary>
    public string? ApplicationName { get; private set; }

    /// <summary>Gets the optional relative path to the source code directory used as Docker build context.</summary>
    public string? SourceCodePath { get; private set; }

    /// <summary>Gets the configurable CI/CD pipeline step options for this Container App.</summary>
    public AppPipelineStepOptions PipelineStepOptions { get; private set; } = new();

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
    /// <param name="acrAuthMode">The optional authentication mode used to pull images from Azure Container Registry.</param>
    /// <param name="acrPullIdentityId">The optional User Assigned Identity used exclusively for ACR image pull.</param>
    /// <param name="dockerImageName">The optional base Docker image name without the tag.</param>
    /// <param name="dockerImageValidated">Whether the user has confirmed the Docker image exists.</param>
    /// <param name="dockerfilePath">The optional relative path to the Dockerfile in the repository.</param>
    /// <param name="applicationName">The optional user-friendly application name for pipeline display.</param>
    /// <param name="sourceCodePath">The optional relative path to the source code directory used as Docker build context.</param>
    public void Update(Name name, Location location, AzureResourceId containerAppEnvironmentId, AzureResourceId? containerRegistryId, AcrAuthMode? acrAuthMode, AzureResourceId? acrPullIdentityId, string? dockerImageName, bool dockerImageValidated, string? dockerfilePath, string? applicationName, string? sourceCodePath) // NOSONAR S107
    {
        SetNameAndLocation(name, location);

        if (IsExisting)
            return;

        ContainerAppEnvironmentId = containerAppEnvironmentId;
        ContainerRegistryId = containerRegistryId;
        AcrAuthMode = containerRegistryId is null ? null : acrAuthMode;
        AcrPullIdentityId = containerRegistryId is null || acrAuthMode?.Value != AcrAuthMode.AcrAuthModeType.ManagedIdentity ? null : acrPullIdentityId;
        DockerImageName = dockerImageName;
        DockerImageValidated = dockerImageValidated;
        DockerfilePath = dockerfilePath;
        ApplicationName = applicationName;
        SourceCodePath = sourceCodePath;
    }

    /// <summary>Sets the pipeline step options for this Container App.</summary>
    public void SetPipelineStepOptions(AppPipelineStepOptions options)
    {
        if (IsExisting) return;
        PipelineStepOptions = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetEnvironmentSettings(ContainerAppEnvironmentSettingsData settings)
    {
        if (IsExisting)
            return;
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == settings.EnvironmentName);

        if (existing is not null)
        {
            existing.Update(settings);
        }
        else
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentSettings.Create(Id, settings));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<ContainerAppEnvironmentSettingsData> settings)
    {
        if (IsExisting)
            return;

        _environmentSettings.Clear();
        foreach (var setting in settings)
        {
            _environmentSettings.Add(
                ContainerAppEnvironmentSettings.Create(Id, setting));
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
    /// <param name="acrAuthMode">The optional authentication mode used to pull images from Azure Container Registry.</param>
    /// <param name="acrPullIdentityId">The optional User Assigned Identity used exclusively for ACR image pull.</param>
    /// <param name="dockerImageName">The optional base Docker image name without the tag.</param>
    /// <param name="dockerfilePath">The optional relative path to the Dockerfile in the repository.</param>
    /// <param name="applicationName">The optional user-friendly application name for pipeline display.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <param name="isExisting">When <c>true</c>, this resource already exists in Azure and is not deployed by this project.</param>
    public static ContainerApp Create( // NOSONAR S107
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId containerAppEnvironmentId,
        AzureResourceId? containerRegistryId,
        AcrAuthMode? acrAuthMode,
        AzureResourceId? acrPullIdentityId = null,
        string? dockerImageName = null,
        string? dockerfilePath = null,
        string? applicationName = null,
        string? sourceCodePath = null,
        IReadOnlyList<ContainerAppEnvironmentSettingsData>? environmentSettings = null,
        bool isExisting = false)
    {
        var resolvedAcrAuthMode = containerRegistryId is null ? null : acrAuthMode;

        var containerApp = new ContainerApp
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting,
            ContainerAppEnvironmentId = containerAppEnvironmentId,
            ContainerRegistryId = containerRegistryId,
            AcrAuthMode = resolvedAcrAuthMode,
            AcrPullIdentityId = resolvedAcrAuthMode?.Value == AcrAuthMode.AcrAuthModeType.ManagedIdentity ? acrPullIdentityId : null,
            DockerImageName = dockerImageName,
            DockerImageValidated = false,
            DockerfilePath = dockerfilePath,
            ApplicationName = applicationName,
            SourceCodePath = sourceCodePath
        };

        if (!isExisting && environmentSettings is not null)
            containerApp.SetAllEnvironmentSettings(environmentSettings);

        return containerApp;
    }
}
