using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;

/// <summary>Command to permanently delete a Log Analytics Workspace resource.</summary>
public record DeleteLogAnalyticsWorkspaceCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
