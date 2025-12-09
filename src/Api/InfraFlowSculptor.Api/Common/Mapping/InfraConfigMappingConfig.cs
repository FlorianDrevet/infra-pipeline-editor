using Mapster;
using InfraFlowSculptor.Application.Authentication.Common;
using InfraFlowSculptor.Contracts.Authentication;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class InfraConfigMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Name, string>()
            .Map(dest => dest, src => src.Value);
    }
}