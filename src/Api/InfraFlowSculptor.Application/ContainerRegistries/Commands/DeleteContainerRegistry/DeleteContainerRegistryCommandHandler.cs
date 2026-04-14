using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.DeleteContainerRegistry;

/// <summary>
/// Handles the <see cref="DeleteContainerRegistryCommand"/> request.
/// </summary>
public sealed class DeleteContainerRegistryCommandHandler(
    IContainerRegistryRepository containerRegistryRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteContainerRegistryCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteContainerRegistryCommand request,
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

        await containerRegistryRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
