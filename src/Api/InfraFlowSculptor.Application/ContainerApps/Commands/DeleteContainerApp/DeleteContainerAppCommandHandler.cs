using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.DeleteContainerApp;

/// <summary>
/// Handles the <see cref="DeleteContainerAppCommand"/> request.
/// </summary>
public sealed class DeleteContainerAppCommandHandler(
    IContainerAppRepository containerAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteContainerAppCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteContainerAppCommand request,
        CancellationToken cancellationToken)
    {
        var containerApp = await containerAppRepository.GetByIdAsync(request.Id, cancellationToken);
        if (containerApp is null)
            return Errors.ContainerApp.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(containerApp.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ContainerApp.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await containerAppRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
