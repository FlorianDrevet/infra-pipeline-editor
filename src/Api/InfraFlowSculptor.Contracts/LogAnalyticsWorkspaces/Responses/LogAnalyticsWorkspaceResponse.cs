using InfraFlowSculptor.Contracts.LogAnalyticsWorkspaces.Requests;

namespace InfraFlowSculptor.Contracts.LogAnalyticsWorkspaces.Responses;

/// <summary>Represents an Azure Log Analytics Workspace resource.</summary>
public record LogAnalyticsWorkspaceResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<LogAnalyticsWorkspaceEnvironmentConfigResponse> EnvironmentSettings
);
