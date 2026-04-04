using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Contracts.ContainerAppEnvironments.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Container App Environment aggregate.</summary>
public sealed class ContainerAppEnvironmentMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateContainerAppEnvironmentRequest, CreateContainerAppEnvironmentCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new ContainerAppEnvironmentEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.WorkloadProfileType,
                        ec.InternalLoadBalancerEnabled,
                        ec.ZoneRedundancyEnabled)).ToList());

        config.NewConfig<(Guid Id, UpdateContainerAppEnvironmentRequest Request), UpdateContainerAppEnvironmentCommand>()
            .MapWith(src => new UpdateContainerAppEnvironmentCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.LogAnalyticsWorkspaceId,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new ContainerAppEnvironmentEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.WorkloadProfileType,
                        ec.InternalLoadBalancerEnabled,
                        ec.ZoneRedundancyEnabled)).ToList()));

        config.NewConfig<ContainerAppEnvironment, ContainerAppEnvironmentResult>()
            .Map(dest => dest.LogAnalyticsWorkspaceId,
                src => src.LogAnalyticsWorkspaceId != null ? src.LogAnalyticsWorkspaceId.Value : (Guid?)null)
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new ContainerAppEnvironmentEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku,
                    es.WorkloadProfileType,
                    es.InternalLoadBalancerEnabled,
                    es.ZoneRedundancyEnabled)).ToList());

        config.NewConfig<ContainerAppEnvironmentEnvironmentConfigData, ContainerAppEnvironmentEnvironmentConfigResponse>()
            .MapWith(src => new ContainerAppEnvironmentEnvironmentConfigResponse(
                src.EnvironmentName,
                src.Sku,
                src.WorkloadProfileType,
                src.InternalLoadBalancerEnabled,
                src.ZoneRedundancyEnabled));
    }
}
