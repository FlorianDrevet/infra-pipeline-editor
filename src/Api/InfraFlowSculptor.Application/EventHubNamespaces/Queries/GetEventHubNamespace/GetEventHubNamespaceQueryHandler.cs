using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Queries;

/// <summary>Handles the <see cref="GetEventHubNamespaceQuery"/> request.</summary>
public class GetEventHubNamespaceQueryHandler(
    IEventHubNamespaceRepository eventHubNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetEventHubNamespaceQuery, EventHubNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<EventHubNamespaceResult>> Handle(
        GetEventHubNamespaceQuery query,
        CancellationToken cancellationToken)
    {
        var eh = await eventHubNamespaceRepository.GetByIdAsync(query.Id, cancellationToken);
        if (eh is null)
            return Errors.EventHubNamespace.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(eh.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.EventHubNamespace.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.EventHubNamespace.NotFoundError(query.Id);

        return mapper.Map<EventHubNamespaceResult>(eh);
    }
}
