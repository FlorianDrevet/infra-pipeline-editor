using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class CommonMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Name, string>()
            .Map(dest => dest, src => src.Value);
        
        config.NewConfig<Location, string>()
            .Map(dest => dest, src => src.Value);

        config.NewConfig<string, Name>()
            .Map(dest => dest, src => new Name(src));
        
        config.NewConfig<string, Location>()
            .Map(dest => dest, src => new Location(Enum.Parse<Location.LocationEnum>(src)));
    }
}