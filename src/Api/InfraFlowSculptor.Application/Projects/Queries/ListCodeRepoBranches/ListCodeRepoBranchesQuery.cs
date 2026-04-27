using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.ListCodeRepoBranches;

/// <summary>Lists all branches in the application-code Git repository configured on a project.</summary>
public record ListCodeRepoBranchesQuery(
    ProjectId ProjectId,
    InfrastructureConfigId? ConfigId = null) : IQuery<IReadOnlyList<GitBranchResult>>;
