using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.DeleteEventHubNamespace;

/// <summary>Handles the <see cref="DeleteEventHubNamespaceCommand"/> request.</summary>
public class DeleteEventHubNamespaceCommandHandler(
    IEventHubNamespaceRepository eventHubNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteEventHubNamespaceCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteEventHubNamespaceCommand request,
        CancellationToken cancellationToken)
    {
        var eh = await eventHubNamespaceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (eh is null)
            return Errors.EventHubNamespace.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(eh.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.EventHubNamespace.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await eventHubNamespaceRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
