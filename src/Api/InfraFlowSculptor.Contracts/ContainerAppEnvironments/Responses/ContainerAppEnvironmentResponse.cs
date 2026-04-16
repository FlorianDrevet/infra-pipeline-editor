using InfraFlowSculptor.Contracts.ContainerAppEnvironments.Requests;

namespace InfraFlowSculptor.Contracts.ContainerAppEnvironments.Responses;

/// <summary>Represents an Azure Container App Environment resource.</summary>
public record ContainerAppEnvironmentResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string? LogAnalyticsWorkspaceId,
    IReadOnlyCollection<ContainerAppEnvironmentEnvironmentConfigResponse> EnvironmentSettings
);
