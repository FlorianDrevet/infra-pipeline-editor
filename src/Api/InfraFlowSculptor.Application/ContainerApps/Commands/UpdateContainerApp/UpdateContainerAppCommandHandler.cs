using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;

/// <summary>
/// Handles the <see cref="UpdateContainerAppCommand"/> request.
/// </summary>
public sealed class UpdateContainerAppCommandHandler(
    IContainerAppRepository containerAppRepository,
    IContainerAppEnvironmentRepository containerAppEnvironmentRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateContainerAppCommand, ContainerAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerAppResult>> Handle(
        UpdateContainerAppCommand request,
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

        // Verify the Container App Environment exists
        var containerAppEnvironmentId = new AzureResourceId(request.ContainerAppEnvironmentId);
        var containerAppEnvironment = await containerAppEnvironmentRepository.GetByIdAsync(containerAppEnvironmentId, cancellationToken);
        if (containerAppEnvironment is null)
            return Errors.ContainerAppEnvironment.NotFoundError(containerAppEnvironmentId);

        containerApp.Update(request.Name, request.Location, containerAppEnvironmentId,
            request.ContainerRegistryId.HasValue
                ? new AzureResourceId(request.ContainerRegistryId.Value)
                : null,
            request.DockerImageName,
            request.DockerfilePath);

        if (request.EnvironmentSettings is not null)
            containerApp.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.CpuCores, ec.MemoryGi, ec.MinReplicas, ec.MaxReplicas, ec.IngressEnabled, ec.IngressTargetPort, ec.IngressExternal, ec.TransportMethod))
                    .ToList());

        var updated = await containerAppRepository.UpdateAsync(containerApp);

        return mapper.Map<ContainerAppResult>(updated);
    }
}
