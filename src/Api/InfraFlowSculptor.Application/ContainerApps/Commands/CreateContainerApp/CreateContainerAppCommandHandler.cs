using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;

/// <summary>Handles the <see cref="CreateContainerAppCommand"/> request.</summary>
public sealed class CreateContainerAppCommandHandler(
    IContainerAppRepository containerAppRepository,
    IContainerAppEnvironmentRepository containerAppEnvironmentRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateContainerAppCommand, ContainerAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerAppResult>> Handle(
        CreateContainerAppCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Verify the Container App Environment exists
        var containerAppEnvironmentId = new AzureResourceId(request.ContainerAppEnvironmentId);
        var containerAppEnvironment = await containerAppEnvironmentRepository.GetByIdAsync(containerAppEnvironmentId, cancellationToken);
        if (containerAppEnvironment is null)
            return Errors.ContainerAppEnvironment.NotFoundError(containerAppEnvironmentId);

        var containerApp = ContainerApp.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            containerAppEnvironmentId,
            request.ContainerRegistryId.HasValue
                ? new AzureResourceId(request.ContainerRegistryId.Value)
                : null,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.ContainerImage, ec.CpuCores, ec.MemoryGi, ec.MinReplicas, ec.MaxReplicas, ec.IngressEnabled, ec.IngressTargetPort, ec.IngressExternal, ec.TransportMethod))
                .ToList());

        var saved = await containerAppRepository.AddAsync(containerApp);

        return mapper.Map<ContainerAppResult>(saved);
    }
}
