using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Contracts.ContainerApps.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Container App feature.</summary>
public sealed class ContainerAppMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateContainerAppRequest, CreateContainerAppCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new ContainerAppEnvironmentConfigData(
                        ec.EnvironmentName, ec.CpuCores, ec.MemoryGi, ec.MinReplicas, ec.MaxReplicas, ec.IngressEnabled, ec.IngressTargetPort, ec.IngressExternal, ec.TransportMethod, ec.ReadinessProbePath, ec.ReadinessProbePort, ec.LivenessProbePath, ec.LivenessProbePort, ec.StartupProbePath, ec.StartupProbePort)).ToList());

        config.NewConfig<(Guid Id, UpdateContainerAppRequest Request), UpdateContainerAppCommand>()
            .MapWith(src => new UpdateContainerAppCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.ContainerAppEnvironmentId,
                src.Request.ContainerRegistryId,
                src.Request.AcrAuthMode,
                src.Request.DockerImageName,
                src.Request.DockerfilePath,
                src.Request.ApplicationName,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new ContainerAppEnvironmentConfigData(
                        ec.EnvironmentName, ec.CpuCores, ec.MemoryGi, ec.MinReplicas, ec.MaxReplicas, ec.IngressEnabled, ec.IngressTargetPort, ec.IngressExternal, ec.TransportMethod, ec.ReadinessProbePath, ec.ReadinessProbePort, ec.LivenessProbePath, ec.LivenessProbePort, ec.StartupProbePath, ec.StartupProbePort)).ToList()));

        config.NewConfig<ContainerApp, ContainerAppResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new ContainerAppEnvironmentConfigData(
                    es.EnvironmentName,
                    es.CpuCores,
                    es.MemoryGi,
                    es.MinReplicas,
                    es.MaxReplicas,
                    es.IngressEnabled,
                    es.IngressTargetPort,
                    es.IngressExternal,
                    es.TransportMethod,
                    es.ReadinessProbePath,
                    es.ReadinessProbePort,
                    es.LivenessProbePath,
                    es.LivenessProbePort,
                    es.StartupProbePath,
                    es.StartupProbePort)).ToList())
            .Map(dest => dest.ContainerAppEnvironmentId, src => src.ContainerAppEnvironmentId.Value)
                    .Map(dest => dest.ContainerRegistryId, src => src.ContainerRegistryId != null ? src.ContainerRegistryId.Value : (Guid?)null)
                    .Map(dest => dest.AcrAuthMode, src => src.AcrAuthMode != null ? src.AcrAuthMode.Value.ToString() : null);

        config.NewConfig<ContainerAppEnvironmentConfigData, ContainerAppEnvironmentConfigResponse>()
            .MapWith(src => new ContainerAppEnvironmentConfigResponse(
                src.EnvironmentName, src.CpuCores, src.MemoryGi, src.MinReplicas, src.MaxReplicas, src.IngressEnabled, src.IngressTargetPort, src.IngressExternal, src.TransportMethod, src.ReadinessProbePath, src.ReadinessProbePort, src.LivenessProbePath, src.LivenessProbePort, src.StartupProbePath, src.StartupProbePort));
    }
}
