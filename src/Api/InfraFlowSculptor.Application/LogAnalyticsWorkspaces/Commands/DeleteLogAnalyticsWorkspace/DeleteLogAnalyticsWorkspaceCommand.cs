using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;

/// <summary>Command to permanently delete a Log Analytics Workspace resource.</summary>
public record DeleteLogAnalyticsWorkspaceCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
