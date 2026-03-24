using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;

/// <summary>Command to create a new Log Analytics Workspace resource inside a Resource Group.</summary>
public record CreateLogAnalyticsWorkspaceCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<LogAnalyticsWorkspaceEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<LogAnalyticsWorkspaceResult>>;
