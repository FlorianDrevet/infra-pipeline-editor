using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Project feature.</summary>
public sealed class ProjectMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        // ProjectId conversions
        config.NewConfig<ProjectId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<Guid, ProjectId>()
            .MapWith(src => ProjectId.Create(src));

        config.NewConfig<ProjectMemberId, Guid>()
            .MapWith(src => src.Value);

        // ProjectMember entity -> ProjectMemberResult
        config.NewConfig<ProjectMember, ProjectMemberResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.EntraId, src => src.User != null ? src.User.EntraId.Value : Guid.Empty)
            .Map(dest => dest.Role, src => src.Role.Value.ToString())
            .Map(dest => dest.FirstName, src => src.User != null ? src.User.Name.FirstName : string.Empty)
            .Map(dest => dest.LastName, src => src.User != null ? src.User.Name.LastName : string.Empty);

        // ProjectMemberResult -> ProjectMemberResponse
        config.NewConfig<ProjectMemberResult, ProjectMemberResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.UserId, src => src.UserId.Value)
            .Map(dest => dest.EntraId, src => src.EntraId)
            .Map(dest => dest.Role, src => src.Role)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName);

        // ── Project Environment Definitions ─────────────────────────────

        // ProjectEnvironmentDefinition entity -> ProjectEnvironmentDefinitionResult
        config.NewConfig<ProjectEnvironmentDefinition, ProjectEnvironmentDefinitionResult>()
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

        // ProjectEnvironmentDefinitionResult -> EnvironmentDefinitionResponse (reuses InfraConfig response)
        config.NewConfig<ProjectEnvironmentDefinitionResult, EnvironmentDefinitionResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Tags, src => src.Tags);

        // ── Project Resource Naming Templates ───────────────────────────

        // ProjectResourceNamingTemplate entity -> ProjectResourceNamingTemplateResult
        config.NewConfig<ProjectResourceNamingTemplate, ProjectResourceNamingTemplateResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Template, src => src.Template.Value);

        // ProjectResourceNamingTemplateResult -> ResourceNamingTemplateResponse (reuses InfraConfig response)
        config.NewConfig<ProjectResourceNamingTemplateResult, ResourceNamingTemplateResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Template, src => src.Template);

        // ── Project Aggregate ───────────────────────────────────────────

        // Project domain -> ProjectResult
        config.NewConfig<Project, ProjectResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Members, src => src.Members)
            .Map(dest => dest.EnvironmentDefinitions, src => src.EnvironmentDefinitions)
            .Map(dest => dest.DefaultNamingTemplate,
                src => src.DefaultNamingTemplate == null ? null : src.DefaultNamingTemplate.Value)
            .Map(dest => dest.ResourceNamingTemplates, src => src.ResourceNamingTemplates);

        // ProjectResult -> ProjectResponse
        config.NewConfig<ProjectResult, ProjectResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Members, src => src.Members)
            .Map(dest => dest.EnvironmentDefinitions, src => src.EnvironmentDefinitions)
            .Map(dest => dest.DefaultNamingTemplate, src => src.DefaultNamingTemplate)
            .Map(dest => dest.ResourceNamingTemplates, src => src.ResourceNamingTemplates);
    }
}
