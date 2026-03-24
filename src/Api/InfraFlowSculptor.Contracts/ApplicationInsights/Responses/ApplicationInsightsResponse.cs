using InfraFlowSculptor.Contracts.ApplicationInsights.Requests;

namespace InfraFlowSculptor.Contracts.ApplicationInsights.Responses;

/// <summary>Represents an Azure Application Insights resource.</summary>
public record ApplicationInsightsResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    Guid LogAnalyticsWorkspaceId,
    IReadOnlyList<ApplicationInsightsEnvironmentConfigResponse> EnvironmentSettings
);
