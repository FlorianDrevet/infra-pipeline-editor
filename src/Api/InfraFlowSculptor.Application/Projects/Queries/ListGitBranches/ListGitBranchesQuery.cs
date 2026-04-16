using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.ListGitBranches;

/// <summary>Lists all branches in the Git repository configured on a project.</summary>
public record ListGitBranchesQuery(ProjectId ProjectId) : IQuery<IReadOnlyCollection<GitBranchResult>>;
