using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Queries;

/// <summary>
/// Handles the <see cref="GetServiceBusNamespaceQuery"/> request
/// and returns the matching Service Bus Namespace if the caller is a member.
/// </summary>
public class GetServiceBusNamespaceQueryHandler(
    IServiceBusNamespaceRepository serviceBusNamespaceRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetServiceBusNamespaceQuery, ServiceBusNamespaceResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ServiceBusNamespaceResult>> Handle(
        GetServiceBusNamespaceQuery query,
        CancellationToken cancellationToken)
    {
        var sb = await serviceBusNamespaceRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (sb is null)
            return Errors.ServiceBusNamespace.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(sb.ResourceGroup!.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.ServiceBusNamespace.NotFoundError(query.Id);

        return mapper.Map<ServiceBusNamespaceResult>(sb);
    }
}
