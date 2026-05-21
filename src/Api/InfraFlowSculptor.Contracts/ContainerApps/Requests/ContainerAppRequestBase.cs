using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ContainerApps.Requests;

/// <summary>Common properties shared by create and update Container App requests.</summary>
public abstract class ContainerAppRequestBase
{
    /// <summary>Display name for the Container App resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Container App will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Identifier of the Container App Environment that hosts this Container App.</summary>
    [Required, GuidValidation]
    public required Guid ContainerAppEnvironmentId { get; init; }

    /// <summary>Optional Container Registry identifier for authenticated image pulls.</summary>
    [GuidValidation]
    public Guid? ContainerRegistryId { get; init; }

    /// <summary>Optional authentication mode used to pull images from Azure Container Registry.</summary>
    [EnumValidation(typeof(AcrAuthMode.AcrAuthModeType))]
    public string? AcrAuthMode { get; init; }

    /// <summary>Optional User Assigned Identity identifier used exclusively for ACR image pull (distinct from the resource-level identity).</summary>
    [GuidValidation]
    public Guid? AcrPullIdentityId { get; init; }

    /// <summary>Optional base Docker image name (e.g., "myregistry.azurecr.io/myapp/api") without the tag.</summary>
    public string? DockerImageName { get; init; }

    /// <summary>Whether the user has confirmed that the Docker image exists in the container registry.</summary>
    public bool DockerImageValidated { get; init; }

    /// <summary>Relative path to the Dockerfile in the repository for pipeline generation.</summary>
    [MaxLength(500), SafeRelativePathValidation]
    public string? DockerfilePath { get; init; }

    /// <summary>User-friendly application name displayed in Azure DevOps pipeline runs.</summary>
    [MaxLength(200)]
    public string? ApplicationName { get; init; }

    /// <summary>Relative path to the source code directory used as Docker build context.</summary>
    [MaxLength(500), SafeRelativePathValidation]
    public string? SourceCodePath { get; init; }

    /// <summary>Optional pipeline step options for CI/CD generation.</summary>
    public PipelineStepOptionsDto? PipelineStepOptions { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<ContainerAppEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}
