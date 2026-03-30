using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusQueue;

/// <summary>
/// Handles the <see cref="AddServiceBusQueueCommand"/> request.
/// </summary>
public class AddServiceBusQueueCommandHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<AddServiceBusQueueCommand, ServiceBusNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ServiceBusNamespaceResult>> Handle(
        AddServiceBusQueueCommand request,
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

        var addResult = sb.AddQueue(request.Name);
        if (addResult.IsError)
            return addResult.Errors;

        await serviceBusNamespaceRepository.UpdateAsync(sb);

        return mapper.Map<ServiceBusNamespaceResult>(sb);
    }
}
