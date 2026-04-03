using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using static InfraFlowSculptor.Domain.Common.ValueObjects.DeploymentMode;

namespace InfraFlowSculptor.Contracts.FunctionApps.Requests;

/// <summary>Common properties shared by create and update Function App requests.</summary>
public abstract class FunctionAppRequestBase
{
    /// <summary>Display name for the Function App resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Function App will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Identifier of the App Service Plan that hosts this Function App.</summary>
    [Required, GuidValidation]
    public required Guid AppServicePlanId { get; init; }

    /// <summary>Runtime stack (DotNet, Node, Python, Java, PowerShell).</summary>
    [Required, EnumValidation(typeof(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum))]
    public required string RuntimeStack { get; init; }

    /// <summary>Runtime version (e.g., "8.0", "20").</summary>
    [Required]
    public required string RuntimeVersion { get; init; }

    /// <summary>Whether the app requires HTTPS only.</summary>
    public bool HttpsOnly { get; init; } = true;

    /// <summary>Deployment mode: "Code" or "Container".</summary>
    [Required, EnumValidation(typeof(DeploymentMode.DeploymentModeType))]
    public required string DeploymentMode { get; init; }

    /// <summary>Optional Container Registry identifier for container deployments.</summary>
    [GuidValidation]
    public Guid? ContainerRegistryId { get; init; }

    /// <summary>Docker image name for container deployments (e.g., "myapp/func").</summary>
    public string? DockerImageName { get; init; }

    /// <summary>Relative path to the Dockerfile in the repository for container pipeline generation.</summary>
    [MaxLength(500)]
    public string? DockerfilePath { get; init; }

    /// <summary>Relative path to the source code folder for code pipeline generation.</summary>
    [MaxLength(500)]
    public string? SourceCodePath { get; init; }

    /// <summary>Optional custom build command for pipeline generation.</summary>
    [MaxLength(1000)]
    public string? BuildCommand { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<FunctionAppEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Function App.</summary>
public class FunctionAppEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional HTTPS-only override.</summary>
    public bool? HttpsOnly { get; init; }

    /// <summary>Optional maximum scale-out instance count override.</summary>
    public int? MaxInstanceCount { get; init; }

    /// <summary>Optional Docker image tag override for this environment (e.g., "latest", "v1.2.3").</summary>
    public string? DockerImageTag { get; init; }
}

/// <summary>Response DTO for a typed per-environment Function App configuration.</summary>
public record FunctionAppEnvironmentConfigResponse(
    string EnvironmentName,
    bool? HttpsOnly,
    int? MaxInstanceCount,
    string? DockerImageTag);
