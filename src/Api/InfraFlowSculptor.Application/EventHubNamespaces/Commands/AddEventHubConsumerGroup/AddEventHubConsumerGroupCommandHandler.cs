using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHubConsumerGroup;

/// <summary>Handles the <see cref="AddEventHubConsumerGroupCommand"/> request.</summary>
public class AddEventHubConsumerGroupCommandHandler(
    IEventHubNamespaceRepository eventHubNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<AddEventHubConsumerGroupCommand, EventHubNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<EventHubNamespaceResult>> Handle(
        AddEventHubConsumerGroupCommand request,
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

        var addResult = eh.AddConsumerGroup(request.EventHubName, request.ConsumerGroupName);
        if (addResult.IsError)
            return addResult.Errors;

        await eventHubNamespaceRepository.UpdateAsync(eh);

        return mapper.Map<EventHubNamespaceResult>(eh);
    }
}
