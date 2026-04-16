using InfraFlowSculptor.Contracts.ApplicationInsights.Requests;

namespace InfraFlowSculptor.Contracts.ApplicationInsights.Responses;

/// <summary>Represents an Azure Application Insights resource.</summary>
public record ApplicationInsightsResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string LogAnalyticsWorkspaceId,
    IReadOnlyCollection<ApplicationInsightsEnvironmentConfigResponse> EnvironmentSettings
);
