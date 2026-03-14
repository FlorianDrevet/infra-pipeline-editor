using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class ResourceGroupMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ResourceGroupId, Guid>()
            .MapWith(src => src.Value);
        
        config.NewConfig<Guid, ResourceGroupId>()
            .MapWith(src => ResourceGroupId.Create(src));

        config.NewConfig<AzureResource, AzureResource>()
            .MapWith(src => src);
    }
}
