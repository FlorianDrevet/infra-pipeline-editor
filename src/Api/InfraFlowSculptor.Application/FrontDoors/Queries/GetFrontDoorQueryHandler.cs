using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FrontDoors.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.FrontDoors.Queries;

/// <summary>Handles retrieval of a single Front Door.</summary>
public sealed class GetFrontDoorQueryHandler(
    IFrontDoorRepository frontDoorRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetFrontDoorQuery, FrontDoorResult>
{
    public async Task<ErrorOr<FrontDoorResult>> Handle(GetFrontDoorQuery query, CancellationToken cancellationToken)
    {
        var fd = await frontDoorRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (fd is null)
            return Errors.FrontDoor.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(fd.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.FrontDoor.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.FrontDoor.NotFoundError(query.Id);

        return mapper.Map<FrontDoorResult>(fd);
    }
}
