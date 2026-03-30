using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListPipelineVariableGroups;

/// <summary>Query to list all pipeline variable groups for an infrastructure configuration.</summary>
/// <param name="InfraConfigId">The infrastructure configuration identifier.</param>
public record ListPipelineVariableGroupsQuery(Guid InfraConfigId)
    : IQuery<List<PipelineVariableGroupResult>>;
