using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;

/// <summary>
/// Application-layer result DTO for the Log Analytics Workspace aggregate.
/// </summary>
public record LogAnalyticsWorkspaceResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<LogAnalyticsWorkspaceEnvironmentConfigData> EnvironmentSettings
);
