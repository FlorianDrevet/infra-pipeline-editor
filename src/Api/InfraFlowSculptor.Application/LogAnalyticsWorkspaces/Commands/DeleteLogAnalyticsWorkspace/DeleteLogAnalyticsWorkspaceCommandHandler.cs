using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;

/// <summary>
/// Handles the <see cref="DeleteLogAnalyticsWorkspaceCommand"/> request.
/// </summary>
public sealed class DeleteLogAnalyticsWorkspaceCommandHandler(
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteLogAnalyticsWorkspaceCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteLogAnalyticsWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var logAnalyticsWorkspace = await logAnalyticsWorkspaceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (logAnalyticsWorkspace is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(logAnalyticsWorkspace.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.LogAnalyticsWorkspace.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await logAnalyticsWorkspaceRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
