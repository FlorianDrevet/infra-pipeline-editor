using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using static InfraFlowSculptor.Domain.Common.ValueObjects.DeploymentMode;

namespace InfraFlowSculptor.Contracts.WebApps.Requests;

/// <summary>Common properties shared by create and update Web App requests.</summary>
public abstract class WebAppRequestBase
{
    /// <summary>Display name for the Web App resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Web App will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Identifier of the App Service Plan that hosts this Web App.</summary>
    [Required, GuidValidation]
    public required Guid AppServicePlanId { get; init; }

    /// <summary>Runtime stack (DotNet, Node, Python, Java, Php).</summary>
    [Required, EnumValidation(typeof(WebAppRuntimeStack.WebAppRuntimeStackEnum))]
    public required string RuntimeStack { get; init; }

    /// <summary>Runtime version (e.g., "8.0", "20").</summary>
    [Required]
    public required string RuntimeVersion { get; init; }

    /// <summary>Whether the app should be always on.</summary>
    public bool AlwaysOn { get; init; } = true;

    /// <summary>Whether the app requires HTTPS only.</summary>
    public bool HttpsOnly { get; init; } = true;

    /// <summary>Deployment mode: "Code" or "Container".</summary>
    [Required, EnumValidation(typeof(DeploymentMode.DeploymentModeType))]
    public required string DeploymentMode { get; init; }

    /// <summary>Optional Container Registry identifier for container deployments.</summary>
    [GuidValidation]
    public Guid? ContainerRegistryId { get; init; }

    /// <summary>Docker image name for container deployments (e.g., "myapp/api").</summary>
    public string? DockerImageName { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<WebAppEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Web App.</summary>
public class WebAppEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional always-on override.</summary>
    public bool? AlwaysOn { get; init; }

    /// <summary>Optional HTTPS-only override.</summary>
    public bool? HttpsOnly { get; init; }

    /// <summary>Optional runtime stack override.</summary>
    [EnumValidation(typeof(WebAppRuntimeStack.WebAppRuntimeStackEnum))]
    public string? RuntimeStack { get; init; }

    /// <summary>Optional runtime version override.</summary>
    public string? RuntimeVersion { get; init; }

    /// <summary>Optional Docker image tag override for this environment (e.g., "latest", "v1.2.3").</summary>
    public string? DockerImageTag { get; init; }
}

/// <summary>Response DTO for a typed per-environment Web App configuration.</summary>
public record WebAppEnvironmentConfigResponse(
    string EnvironmentName,
    bool? AlwaysOn,
    bool? HttpsOnly,
    string? RuntimeStack,
    string? RuntimeVersion,
    string? DockerImageTag);
