using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;

/// <summary>Command to create a new Log Analytics Workspace resource inside a Resource Group.</summary>
public record CreateLogAnalyticsWorkspaceCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyCollection<LogAnalyticsWorkspaceEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<LogAnalyticsWorkspaceResult>;
