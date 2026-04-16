using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;

/// <summary>Command to update an existing Application Insights resource.</summary>
public record UpdateApplicationInsightsCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid LogAnalyticsWorkspaceId,
    IReadOnlyCollection<ApplicationInsightsEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<ApplicationInsightsResult>;
