using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.WebApps.Common;

/// <summary>Application-layer result for a Web App operation.</summary>
public record WebAppResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool AlwaysOn,
    bool HttpsOnly,
    string DeploymentMode,
    Guid? ContainerRegistryId,
    string? AcrAuthMode,
    string? DockerImageName,
    string? DockerfilePath,
    string? SourceCodePath,
    string? BuildCommand,
    string? ApplicationName,
    IReadOnlyList<WebAppEnvironmentConfigData> EnvironmentSettings,
    bool IsExisting = false
);
