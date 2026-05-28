using InfraFlowSculptor.Application.NetworkingProfiles;
using InfraFlowSculptor.Contracts.NetworkingProfiles.Responses;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster configuration for networking profile mappings.</summary>
public sealed class NetworkingProfileMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<NetworkingProfileResult, NetworkingProfileResponse>();
    }
}
