using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Models;
using MapsterMapper;

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
            !string.IsNullOrWhiteSpace(request.AcrAuthMode)
                ? new AcrAuthMode(Enum.Parse<AcrAuthMode.AcrAuthModeType>(request.AcrAuthMode))
                : null,
            request.AcrPullIdentityId.HasValue
                ? new AzureResourceId(request.AcrPullIdentityId.Value)
                : null,
            request.DockerImageName,
            request.DockerImageValidated,
            request.DockerfilePath,
            request.ApplicationName,
            request.SourceCodePath);

        if (request.EnvironmentSettings is not null)
            containerApp.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(MapEnvironmentSettings)
                    .ToList());

        if (request.PipelineStepOptions is { } opts)
        {
            containerApp.PipelineStepOptions.Update(PipelineStepOptionsDataMapper.ToDomainData(opts));
        }

        var updated = containerAppRepository.Update(containerApp);

        return mapper.Map<ContainerAppResult>(updated);
    }

    private static ContainerAppEnvironmentSettingsData MapEnvironmentSettings(ContainerAppEnvironmentConfigData settings)
        => new(
            settings.EnvironmentName,
            settings.CpuCores,
            settings.MemoryGi,
            settings.MinReplicas,
            settings.MaxReplicas,
            settings.IngressEnabled,
            settings.IngressTargetPort,
            settings.IngressExternal,
            settings.TransportMethod,
            settings.ReadinessProbePath,
            settings.ReadinessProbePort,
            settings.LivenessProbePath,
            settings.LivenessProbePort,
            settings.StartupProbePath,
            settings.StartupProbePort,
            settings.ContainerRegistryServiceConnection);
}
