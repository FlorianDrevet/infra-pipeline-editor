using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Models;
using MapsterMapper;
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
            !string.IsNullOrWhiteSpace(request.AcrAuthMode)
                ? new AcrAuthMode(Enum.Parse<AcrAuthMode.AcrAuthModeType>(request.AcrAuthMode))
                : null,
            request.AcrPullIdentityId.HasValue
                ? new AzureResourceId(request.AcrPullIdentityId.Value)
                : null,
            request.DockerImageName,
            request.DockerfilePath,
            request.ApplicationName,
            request.SourceCodePath,
            request.EnvironmentSettings?
                .Select(MapEnvironmentSettings)
                .ToList(),
            isExisting: request.IsExisting);

        var saved = containerAppRepository.Add(containerApp);

        if (request.PipelineStepOptions is { } opts)
        {
            containerApp.PipelineStepOptions.Update(PipelineStepOptionsDataMapper.ToDomainData(opts));
        }

        return mapper.Map<ContainerAppResult>(saved);
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
