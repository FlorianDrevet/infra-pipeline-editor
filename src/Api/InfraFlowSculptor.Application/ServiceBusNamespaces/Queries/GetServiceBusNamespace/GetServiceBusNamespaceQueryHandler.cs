using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Queries;

/// <summary>
/// Handles the <see cref="GetServiceBusNamespaceQuery"/> request
/// and returns the matching Service Bus Namespace if the caller is a member.
/// </summary>
public class GetServiceBusNamespaceQueryHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetServiceBusNamespaceQuery, ServiceBusNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ServiceBusNamespaceResult>> Handle(
        GetServiceBusNamespaceQuery query,
        CancellationToken cancellationToken)
    {
        var sb = await serviceBusNamespaceRepository.GetByIdAsync(query.Id, cancellationToken);
        if (sb is null)
            return Errors.ServiceBusNamespace.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(sb.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ServiceBusNamespace.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return mapper.Map<ServiceBusNamespaceResult>(sb);
    }
}
