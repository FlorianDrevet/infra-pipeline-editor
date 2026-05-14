using InfraFlowSculptor.Application.FrontDoors.Commands.CreateFrontDoor;
using InfraFlowSculptor.Application.FrontDoors.Commands.UpdateFrontDoor;
using InfraFlowSculptor.Application.FrontDoors.Common;
using InfraFlowSculptor.Contracts.FrontDoors.Requests;
using InfraFlowSculptor.Contracts.FrontDoors.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FrontDoorAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster configuration for Front Door mappings.</summary>
public sealed class FrontDoorMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateFrontDoorRequest, CreateFrontDoorCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new FrontDoorEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku)).ToList());

        config.NewConfig<(Guid Id, UpdateFrontDoorRequest Request), UpdateFrontDoorCommand>()
            .MapWith(src => new UpdateFrontDoorCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.WafPolicyEnabled,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new FrontDoorEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku)).ToList()));

        config.NewConfig<FrontDoor, FrontDoorResult>()
            .Map(dest => dest.Origins,
                src => src.Origins.Select(o => new FrontDoorOriginData(
                    o.Id.Value.ToString(),
                    o.TargetResourceId.Value.ToString(),
                    o.HostName,
                    o.PrivateLinkEnabled,
                    o.Weight,
                    o.Priority)).ToList())
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new FrontDoorEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku.Value.ToString())).ToList());

        config.NewConfig<FrontDoorOriginData, FrontDoorOriginResponse>()
            .MapWith(src => new FrontDoorOriginResponse(
                src.Id, src.TargetResourceId, src.HostName, src.PrivateLinkEnabled, src.Weight, src.Priority));

        config.NewConfig<FrontDoorEnvironmentConfigData, FrontDoorEnvironmentConfigResponse>()
            .MapWith(src => new FrontDoorEnvironmentConfigResponse(
                src.EnvironmentName, src.Sku));
    }
}
