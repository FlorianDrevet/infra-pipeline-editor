using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Contracts.ResourceGroups.Responses;
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

        config.NewConfig<AzureResourceResult, AzureResourceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Location, src => src.Location.Value.ToString())
            .Map(dest => dest.ParentResourceId, src => src.ParentResourceId != null ? src.ParentResourceId.Value : (Guid?)null)
            .Map(dest => dest.ConfiguredEnvironments, src => src.ConfiguredEnvironments);
    }
}
