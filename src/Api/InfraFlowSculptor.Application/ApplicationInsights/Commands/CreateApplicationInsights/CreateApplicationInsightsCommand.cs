using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;

/// <summary>Command to create a new Application Insights resource inside a Resource Group.</summary>
public record CreateApplicationInsightsCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid LogAnalyticsWorkspaceId,
    IReadOnlyList<ApplicationInsightsEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<ApplicationInsightsResult>;
