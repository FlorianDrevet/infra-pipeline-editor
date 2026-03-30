using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.FunctionApps.Common;

/// <summary>Application-layer result for a Function App operation.</summary>
public record FunctionAppResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool HttpsOnly,
    string DeploymentMode,
    Guid? ContainerRegistryId,
    string? DockerImageName,
    IReadOnlyList<FunctionAppEnvironmentConfigData> EnvironmentSettings);
