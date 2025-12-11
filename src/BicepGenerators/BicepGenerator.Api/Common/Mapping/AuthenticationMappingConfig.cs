using Mapster;
using BicepGenerator.Application.Authentication.Common;
using BicepGenerator.Contracts.Authentication;

namespace BicepGenerator.Api.Common.Mapping;

public class AuthenticationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AuthenticationResult, AuthenticationResponse>()
            .BeforeMapping((src, dest) => Console.WriteLine(src))
            .Map(dest => dest.Token, src => src.Token)
            .Map(dest => dest.Id, src => src.User.Id.Value)
            .Map(dest => dest, src => src.User);
    }
}