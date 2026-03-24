using ErrorOr;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;

/// <summary>Command to update an existing Application Insights resource.</summary>
public record UpdateApplicationInsightsCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid LogAnalyticsWorkspaceId,
    IReadOnlyList<ApplicationInsightsEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<ApplicationInsightsResult>>;
