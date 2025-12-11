using Mapster;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class InfraConfigMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InfrastructureConfigId, Guid>()
            .MapWith(src => src.Value);
        
        config.NewConfig<Guid, InfrastructureConfigId>()
            .MapWith(src => InfrastructureConfigId.Create(src));
    }
}