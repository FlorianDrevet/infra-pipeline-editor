using InfraFlowSculptor.Contracts.ContainerAppEnvironments.Requests;

namespace InfraFlowSculptor.Contracts.ContainerAppEnvironments.Responses;

/// <summary>Represents an Azure Container App Environment resource.</summary>
public record ContainerAppEnvironmentResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<ContainerAppEnvironmentEnvironmentConfigResponse> EnvironmentSettings
);
