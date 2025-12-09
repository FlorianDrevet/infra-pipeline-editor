using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class CommonMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AzureResourceId, Guid>()
            .MapWith(src => src.Value);
        
        config.NewConfig<Guid, AzureResourceId>()
            .MapWith(src => AzureResourceId.Create(src));
        
        config.ForType<Name, string>()
            .MapWith(src => src.Value);
        
        config.ForType<Location, string>()
            .MapWith(src => src.Value.ToString());

        config.ForType<string, Name>()
            .MapWith(src => new Name(src));
        
        config.ForType<string, Location>()
            .MapWith(src => new Location(Enum.Parse<Location.LocationEnum>(src)));
    }
}