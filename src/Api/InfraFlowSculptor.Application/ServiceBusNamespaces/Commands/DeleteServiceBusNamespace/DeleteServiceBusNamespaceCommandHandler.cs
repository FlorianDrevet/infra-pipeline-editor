using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.DeleteServiceBusNamespace;

/// <summary>
/// Handles the <see cref="DeleteServiceBusNamespaceCommand"/> request.
/// </summary>
public class DeleteServiceBusNamespaceCommandHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteServiceBusNamespaceCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteServiceBusNamespaceCommand request,
        CancellationToken cancellationToken)
    {
        var sb = await serviceBusNamespaceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sb is null)
            return Errors.ServiceBusNamespace.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(sb.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ServiceBusNamespace.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await serviceBusNamespaceRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
