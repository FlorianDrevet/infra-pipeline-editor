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
            .Map(dest => dest.GroupName, src => src.GroupName)
            .Map(dest => dest.Variables, src => src.Variables);

        config.NewConfig<PipelineVariableUsageResult, PipelineVariableUsageResponse>()
            .Map(dest => dest.PipelineVariableName, src => src.PipelineVariableName)
            .Map(dest => dest.AppSettingName, src => src.AppSettingName)
            .Map(dest => dest.ResourceName, src => src.ResourceName)
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.ConfigName, src => src.ConfigName);
    }
}
