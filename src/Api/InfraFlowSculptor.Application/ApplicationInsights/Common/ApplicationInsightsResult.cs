using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ApplicationInsights.Common;

/// <summary>
/// Application-layer result DTO for the Application Insights aggregate.
/// </summary>
public record ApplicationInsightsResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid LogAnalyticsWorkspaceId,
    IReadOnlyCollection<ApplicationInsightsEnvironmentConfigData> EnvironmentSettings);
