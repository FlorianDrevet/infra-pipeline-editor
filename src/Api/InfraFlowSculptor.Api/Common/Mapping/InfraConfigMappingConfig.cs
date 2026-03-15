using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class InfraConfigMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InfrastructureConfigId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<Guid, InfrastructureConfigId>()
            .MapWith(src => InfrastructureConfigId.Create(src));

        config.NewConfig<MemberId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<UserId, Guid>()
            .MapWith(src => src.Value);

        // Member entity -> MemberResult
        config.NewConfig<Member, MemberResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Role, src => src.Role.Value.ToString());

        // MemberResult -> MemberResponse
        config.NewConfig<MemberResult, MemberResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.UserId, src => src.UserId.Value)
            .Map(dest => dest.Role, src => src.Role);

        // GetInfrastructureConfigResult -> InfrastructureConfigResponse
        config.NewConfig<GetInfrastructureConfigResult, InfrastructureConfigResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Members, src => src.Members);

        // InfrastructureConfig domain -> GetInfrastructureConfigResult
        config.NewConfig<Domain.InfrastructureConfigAggregate.InfrastructureConfig, GetInfrastructureConfigResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Members, src => src.Members);
    }
}