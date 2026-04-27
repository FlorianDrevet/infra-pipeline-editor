using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using MapsterMapper;
using ErrorOr;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;

/// <summary>
/// Handles the <see cref="CreateContainerRegistryCommand"/> request.
/// </summary>
public sealed class CreateContainerRegistryCommandHandler(
    IContainerRegistryRepository containerRegistryRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateContainerRegistryCommand, ContainerRegistryResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerRegistryResult>> Handle(
        CreateContainerRegistryCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var containerRegistry = ContainerRegistry.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.Sku, ec.AdminUserEnabled, ec.PublicNetworkAccess, ec.ZoneRedundancy))
                .ToList(),
            isExisting: request.IsExisting);

        var saved = await containerRegistryRepository.AddAsync(containerRegistry);

        return mapper.Map<ContainerRegistryResult>(saved);
    }
}
