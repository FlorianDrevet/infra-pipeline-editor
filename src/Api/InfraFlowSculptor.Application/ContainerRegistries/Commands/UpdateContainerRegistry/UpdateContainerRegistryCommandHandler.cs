using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.UpdateContainerRegistry;

/// <summary>
/// Handles the <see cref="UpdateContainerRegistryCommand"/> request.
/// </summary>
public sealed class UpdateContainerRegistryCommandHandler(
    IContainerRegistryRepository containerRegistryRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateContainerRegistryCommand, ContainerRegistryResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerRegistryResult>> Handle(
        UpdateContainerRegistryCommand request,
        CancellationToken cancellationToken)
    {
        var containerRegistry = await containerRegistryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (containerRegistry is null)
            return Errors.ContainerRegistry.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(containerRegistry.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ContainerRegistry.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        containerRegistry.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            containerRegistry.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.Sku, ec.AdminUserEnabled, ec.PublicNetworkAccess, ec.ZoneRedundancy))
                    .ToList());

        var updated = await containerRegistryRepository.UpdateAsync(containerRegistry);

        return mapper.Map<ContainerRegistryResult>(updated);
    }
}
