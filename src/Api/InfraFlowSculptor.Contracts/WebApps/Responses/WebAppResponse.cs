using InfraFlowSculptor.Contracts.WebApps.Requests;

namespace InfraFlowSculptor.Contracts.WebApps.Responses;

/// <summary>Represents an Azure Web App resource.</summary>
/// <param name="Id">Unique identifier of the Web App.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Web App.</param>
/// <param name="Location">Azure region where the Web App is deployed.</param>
/// <param name="AppServicePlanId">Identifier of the hosting App Service Plan.</param>
/// <param name="RuntimeStack">Runtime stack (e.g., "DotNet", "Node").</param>
/// <param name="RuntimeVersion">Runtime version (e.g., "8.0", "20").</param>
/// <param name="AlwaysOn">Whether the app is always on.</param>
/// <param name="HttpsOnly">Whether the app requires HTTPS only.</param>
/// <param name="DeploymentMode">Deployment mode (Code or Container).</param>
/// <param name="ContainerRegistryId">Optional Container Registry identifier.</param>
/// <param name="AcrAuthMode">Optional authentication mode used to pull images from Azure Container Registry.</param>
/// <param name="DockerImageName">Docker image name for container deployments.</param>
/// <param name="DockerfilePath">Relative path to the Dockerfile in the repository.</param>
/// <param name="SourceCodePath">Relative path to the source code folder.</param>
/// <param name="BuildCommand">Optional custom build command.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record WebAppResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool AlwaysOn,
    bool HttpsOnly,
    string DeploymentMode,
    string? ContainerRegistryId,
    string? AcrAuthMode,
    string? DockerImageName,
    string? DockerfilePath,
    string? SourceCodePath,
    string? BuildCommand,
    string? ApplicationName,
    IReadOnlyList<WebAppEnvironmentConfigResponse> EnvironmentSettings,

    bool IsExisting = false

);
