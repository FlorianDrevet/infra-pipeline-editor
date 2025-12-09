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
            .Map(dest => dest, src => src.Value.ToString());
        
        config.NewConfig<string, Sku>()
            .Map(dest => dest, src => new Sku(Enum.Parse<Sku.SkuEnum>(src)));
    }
}