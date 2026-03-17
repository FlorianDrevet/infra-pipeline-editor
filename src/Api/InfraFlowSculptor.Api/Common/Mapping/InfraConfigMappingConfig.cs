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

        config.NewConfig<EnvironmentDefinitionId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<ResourceNamingTemplateId, Guid>()
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

        // EnvironmentDefinition entity -> EnvironmentDefinitionResult
        config.NewConfig<EnvironmentDefinition, EnvironmentDefinitionResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Prefix, src => src.Prefix.Value)
            .Map(dest => dest.Suffix, src => src.Suffix.Value)
            .Map(dest => dest.Location, src => src.Location.Value.ToString())
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.SubscriptionId, src => src.SubscriptionId.Value)
            .Map(dest => dest.Order, src => src.Order.Value)
            .Map(dest => dest.RequiresApproval, src => src.RequiresApproval.Value)
            .Map(dest => dest.Tags, src => src.Tags);

        // Tag -> TagResult
        config.NewConfig<Tag, TagResult>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Value, src => src.Value);

        // EnvironmentDefinitionResult -> EnvironmentDefinitionResponse
        config.NewConfig<EnvironmentDefinitionResult, EnvironmentDefinitionResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Tags, src => src.Tags);

        // TagResult -> TagResponse
        config.NewConfig<TagResult, TagResponse>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Value, src => src.Value);

        // ResourceNamingTemplate entity -> ResourceNamingTemplateResult
        config.NewConfig<ResourceNamingTemplate, ResourceNamingTemplateResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Template, src => src.Template.Value);

        // ResourceNamingTemplateResult -> ResourceNamingTemplateResponse
        config.NewConfig<ResourceNamingTemplateResult, ResourceNamingTemplateResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Template, src => src.Template);

        // GetInfrastructureConfigResult -> InfrastructureConfigResponse
        config.NewConfig<GetInfrastructureConfigResult, InfrastructureConfigResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.DefaultNamingTemplate, src => src.DefaultNamingTemplate)
            .Map(dest => dest.Members, src => src.Members)
            .Map(dest => dest.EnvironmentDefinitions, src => src.EnvironmentDefinitions)
            .Map(dest => dest.ResourceNamingTemplates, src => src.ResourceNamingTemplates);

        // InfrastructureConfig domain -> GetInfrastructureConfigResult
        config.NewConfig<Domain.InfrastructureConfigAggregate.InfrastructureConfig, GetInfrastructureConfigResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.DefaultNamingTemplate, src => src.DefaultNamingTemplate == null ? null : src.DefaultNamingTemplate.Value)
            .Map(dest => dest.Members, src => src.Members)
            .Map(dest => dest.EnvironmentDefinitions, src => src.EnvironmentDefinitions)
            .Map(dest => dest.ResourceNamingTemplates, src => src.ResourceNamingTemplates);
    }
}
