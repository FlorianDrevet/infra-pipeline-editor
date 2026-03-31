using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;
using MapsterMapper;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;

/// <summary>Handles the <see cref="RemoveEventHubConsumerGroupCommand"/> request.</summary>
public class RemoveEventHubConsumerGroupCommandHandler(
    IEventHubNamespaceRepository eventHubNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<RemoveEventHubConsumerGroupCommand, EventHubNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<EventHubNamespaceResult>> Handle(
        RemoveEventHubConsumerGroupCommand request,
        CancellationToken cancellationToken)
    {
        var eh = await eventHubNamespaceRepository.GetByIdAsync(request.EventHubNamespaceId, cancellationToken);
        if (eh is null)
            return Errors.EventHubNamespace.NotFoundError(request.EventHubNamespaceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(eh.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.EventHubNamespace.NotFoundError(request.EventHubNamespaceId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var removeResult = eh.RemoveConsumerGroup(new EventHubConsumerGroupId(request.ConsumerGroupId));
        if (removeResult.IsError)
            return removeResult.Errors;

        await eventHubNamespaceRepository.UpdateAsync(eh);

        return mapper.Map<EventHubNamespaceResult>(eh);
    }
}
