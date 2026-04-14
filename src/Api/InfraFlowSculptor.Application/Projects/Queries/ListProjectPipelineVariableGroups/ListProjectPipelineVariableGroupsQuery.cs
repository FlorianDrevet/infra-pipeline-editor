using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;

/// <summary>Query to list all pipeline variable groups for a project (shared across all configurations).</summary>
/// <param name="ProjectId">The project identifier.</param>
public record ListProjectPipelineVariableGroupsQuery(ProjectId ProjectId)
    : IQuery<List<ProjectPipelineVariableGroupResult>>;
