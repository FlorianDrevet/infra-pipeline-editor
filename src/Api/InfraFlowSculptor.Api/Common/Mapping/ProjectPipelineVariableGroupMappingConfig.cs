using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Contracts.Projects.Responses;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for project-level pipeline variable group request/response types.</summary>
public sealed class ProjectPipelineVariableGroupMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectPipelineVariableGroupResult, ProjectPipelineVariableGroupResponse>()
            .Map(dest => dest.Id, src => src.GroupId.ToString())
            .Map(dest => dest.GroupName, src => src.GroupName);
    }
}
