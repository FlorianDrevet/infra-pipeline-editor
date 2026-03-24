using ErrorOr;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;

/// <summary>Command to update an existing Log Analytics Workspace resource.</summary>
public record UpdateLogAnalyticsWorkspaceCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyList<LogAnalyticsWorkspaceEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<LogAnalyticsWorkspaceResult>>;
