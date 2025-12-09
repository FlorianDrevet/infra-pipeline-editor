using Mapster;
using InfraFlowSculptor.Application.Authentication.Common;
using InfraFlowSculptor.Contracts.Authentication;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class AuthenticationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AuthenticationResult, AuthenticationResponse>()
            .Map(dest => dest.Token, src => src.Token)
            .Map(dest => dest.Id, src => src.User.Id.Value)
            .Map(dest => dest, src => src.User);
    }
}