using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusQueue;

/// <summary>
/// Handles the <see cref="RemoveServiceBusQueueCommand"/> request.
/// </summary>
public class RemoveServiceBusQueueCommandHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<RemoveServiceBusQueueCommand, ServiceBusNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ServiceBusNamespaceResult>> Handle(
        RemoveServiceBusQueueCommand request,
        CancellationToken cancellationToken)
    {
        var sb = await serviceBusNamespaceRepository.GetByIdAsync(request.ServiceBusNamespaceId, cancellationToken);
        if (sb is null)
            return Errors.ServiceBusNamespace.NotFoundError(request.ServiceBusNamespaceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(sb.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ServiceBusNamespace.NotFoundError(request.ServiceBusNamespaceId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var removeResult = sb.RemoveQueue(new ServiceBusQueueId(request.QueueId));
        if (removeResult.IsError)
            return removeResult.Errors;

        await serviceBusNamespaceRepository.UpdateAsync(sb);

        return mapper.Map<ServiceBusNamespaceResult>(sb);
    }
}
