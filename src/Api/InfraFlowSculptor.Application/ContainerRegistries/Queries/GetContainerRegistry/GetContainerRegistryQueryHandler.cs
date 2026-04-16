using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.ContainerRegistries.Queries.GetContainerRegistry;

/// <summary>
/// Handles the <see cref="GetContainerRegistryQuery"/> request
/// and returns the matching Container Registry if the caller is a member.
/// </summary>
public sealed class GetContainerRegistryQueryHandler(
    IContainerRegistryRepository containerRegistryRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetContainerRegistryQuery, ContainerRegistryResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerRegistryResult>> Handle(
        GetContainerRegistryQuery query,
        CancellationToken cancellationToken)
    {
        var containerRegistry = await containerRegistryRepository.GetByIdAsync(query.Id, cancellationToken);
        if (containerRegistry is null)
            return Errors.ContainerRegistry.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(containerRegistry.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ContainerRegistry.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return mapper.Map<ContainerRegistryResult>(containerRegistry);
    }
}
