using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;

/// <summary>
/// Handles the <see cref="AddServiceBusTopicSubscriptionCommand"/> request.
/// </summary>
public class AddServiceBusTopicSubscriptionCommandHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<AddServiceBusTopicSubscriptionCommand, ServiceBusNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ServiceBusNamespaceResult>> Handle(
        AddServiceBusTopicSubscriptionCommand request,
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

        var addResult = sb.AddTopicSubscription(request.TopicName, request.SubscriptionName);
        if (addResult.IsError)
            return addResult.Errors;

        await serviceBusNamespaceRepository.UpdateAsync(sb);

        return mapper.Map<ServiceBusNamespaceResult>(sb);
    }
}
