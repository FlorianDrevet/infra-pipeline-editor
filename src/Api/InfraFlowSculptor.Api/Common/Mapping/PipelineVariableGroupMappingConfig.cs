using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListPipelineVariableGroups;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for pipeline variable group request/response types.</summary>
public sealed class PipelineVariableGroupMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PipelineVariableGroupResult, PipelineVariableGroupResponse>()
            .Map(dest => dest.Id, src => src.GroupId.ToString())
            .Map(dest => dest.GroupName, src => src.GroupName)
            .Map(dest => dest.Mappings, src => src.Mappings);

        config.NewConfig<PipelineVariableMappingResult, PipelineVariableMappingResponse>()
            .Map(dest => dest.Id, src => src.MappingId.ToString())
            .Map(dest => dest.PipelineVariableName, src => src.PipelineVariableName)
            .Map(dest => dest.BicepParameterName, src => src.BicepParameterName);
    }
}
