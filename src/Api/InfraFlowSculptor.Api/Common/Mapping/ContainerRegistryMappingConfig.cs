using InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.UpdateContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Contracts.ContainerRegistries.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Container Registry aggregate.</summary>
public sealed class ContainerRegistryMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateContainerRegistryRequest, CreateContainerRegistryCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new ContainerRegistryEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.AdminUserEnabled,
                        ec.PublicNetworkAccess,
                        ec.ZoneRedundancy)).ToList());

        config.NewConfig<(Guid Id, UpdateContainerRegistryRequest Request), UpdateContainerRegistryCommand>()
            .MapWith(src => new UpdateContainerRegistryCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new ContainerRegistryEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.AdminUserEnabled,
                        ec.PublicNetworkAccess,
                        ec.ZoneRedundancy)).ToList()));

        config.NewConfig<ContainerRegistry, ContainerRegistryResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new ContainerRegistryEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku,
                    es.AdminUserEnabled,
                    es.PublicNetworkAccess,
                    es.ZoneRedundancy)).ToList());

        config.NewConfig<ContainerRegistryEnvironmentConfigData, ContainerRegistryEnvironmentConfigResponse>()
            .MapWith(src => new ContainerRegistryEnvironmentConfigResponse(
                src.EnvironmentName,
                src.Sku,
                src.AdminUserEnabled,
                src.PublicNetworkAccess,
                src.ZoneRedundancy));
    }
}
