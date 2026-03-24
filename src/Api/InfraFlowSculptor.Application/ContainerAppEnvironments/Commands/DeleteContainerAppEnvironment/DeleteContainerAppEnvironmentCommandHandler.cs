using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;

/// <summary>
/// Handles the <see cref="DeleteContainerAppEnvironmentCommand"/> request.
/// </summary>
public sealed class DeleteContainerAppEnvironmentCommandHandler(
    IContainerAppEnvironmentRepository containerAppEnvironmentRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteContainerAppEnvironmentCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteContainerAppEnvironmentCommand request,
        CancellationToken cancellationToken)
    {
        var containerAppEnvironment = await containerAppEnvironmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (containerAppEnvironment is null)
            return Errors.ContainerAppEnvironment.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(containerAppEnvironment.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ContainerAppEnvironment.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await containerAppEnvironmentRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
