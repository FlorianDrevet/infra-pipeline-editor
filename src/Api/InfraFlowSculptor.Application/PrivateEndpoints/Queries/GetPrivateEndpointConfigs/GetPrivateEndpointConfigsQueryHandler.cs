using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Queries.GetPrivateEndpointConfigs;

/// <summary>Handler for getting private endpoint configurations.</summary>
public sealed class GetPrivateEndpointConfigsQueryHandler(
    IAzureResourceRepository resourceRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper) : IQueryHandler<GetPrivateEndpointConfigsQuery, IReadOnlyList<PrivateEndpointConfigResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<PrivateEndpointConfigResult>>> Handle(
        GetPrivateEndpointConfigsQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.GetByIdWithPrivateEndpointsReadOnlyAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Errors.AzureResource.NotFound(request.ResourceId);

        var authResult = await accessService.VerifyReadAccessAsync(
            resource.ResourceGroup!.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return resource.PrivateEndpointConfigs
            .Select(c => mapper.Map<PrivateEndpointConfigResult>(c))
            .ToList();
    }
}
