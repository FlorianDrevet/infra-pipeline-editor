using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Contracts.CustomDomains.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the custom domain feature.</summary>
public sealed class CustomDomainMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomDomainResult, CustomDomainResponse>()
            .MapWith(src => new CustomDomainResponse(
                src.Id.Value.ToString(),
                src.ResourceId.Value.ToString(),
                src.EnvironmentName,
                src.DomainName,
                src.BindingType));
    }
}
