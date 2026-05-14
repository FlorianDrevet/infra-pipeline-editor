using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Contracts.PrivateEndpoints.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for Private Endpoint types.</summary>
public sealed class PrivateEndpointMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PrivateEndpointConfig, PrivateEndpointConfigResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ResourceId, src => src.ResourceId)
            .Map(dest => dest.SubnetId, src => src.SubnetId)
            .Map(dest => dest.GroupId, src => src.GroupId)
            .Map(dest => dest.AutoApproval, src => src.AutoApproval)
            .Map(dest => dest.PrivateDnsZoneId, src => src.PrivateDnsZoneId)
            .Map(dest => dest.CustomNetworkInterfaceName, src => src.CustomNetworkInterfaceName);

        config.NewConfig<PrivateEndpointConfigResult, PrivateEndpointConfigResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.ResourceId, src => src.ResourceId.Value.ToString())
            .Map(dest => dest.SubnetId, src => src.SubnetId.Value.ToString())
            .Map(dest => dest.GroupId, src => src.GroupId)
            .Map(dest => dest.AutoApproval, src => src.AutoApproval)
            .Map(dest => dest.PrivateDnsZoneId,
                src => src.PrivateDnsZoneId != null ? src.PrivateDnsZoneId.Value.ToString() : null)
            .Map(dest => dest.CustomNetworkInterfaceName, src => src.CustomNetworkInterfaceName);
    }
}
