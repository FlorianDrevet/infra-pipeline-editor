using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;

/// <summary>Command to update an existing Log Analytics Workspace resource.</summary>
public record UpdateLogAnalyticsWorkspaceCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyCollection<LogAnalyticsWorkspaceEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<LogAnalyticsWorkspaceResult>;
