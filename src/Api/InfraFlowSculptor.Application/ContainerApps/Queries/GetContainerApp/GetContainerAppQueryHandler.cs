using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;

/// <summary>
/// Handles the <see cref="GetContainerAppQuery"/> request
/// and returns the matching Container App if the caller is a member.
/// </summary>
public sealed class GetContainerAppQueryHandler(
    IContainerAppRepository containerAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetContainerAppQuery, ContainerAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerAppResult>> Handle(
        GetContainerAppQuery query,
        CancellationToken cancellationToken)
    {
        var containerApp = await containerAppRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (containerApp is null)
            return Errors.ContainerApp.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(containerApp.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ContainerApp.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.ContainerApp.NotFoundError(query.Id);

        return mapper.Map<ContainerAppResult>(containerApp);
    }
}
