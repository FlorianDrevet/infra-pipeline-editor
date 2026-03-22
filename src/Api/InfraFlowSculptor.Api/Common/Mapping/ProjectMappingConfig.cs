using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
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

        // Project domain -> ProjectResult
        config.NewConfig<Project, ProjectResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Members, src => src.Members);

        // ProjectResult -> ProjectResponse
        config.NewConfig<ProjectResult, ProjectResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Members, src => src.Members);
    }
}
