using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class KeyVaultMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Sku, string>()
            .MapWith(src => src.Value.ToString());
        
        config.NewConfig<string, Sku>()
            .MapWith(src => new Sku(Enum.Parse<Sku.SkuEnum>(src)));
    }
}