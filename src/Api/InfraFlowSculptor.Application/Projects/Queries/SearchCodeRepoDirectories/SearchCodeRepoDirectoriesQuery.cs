using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoDirectories;

/// <summary>Lists directories in the application-code Git repository on a specific branch.</summary>
public record SearchCodeRepoDirectoriesQuery(
    ProjectId ProjectId,
    string Branch,
    string? PathPrefix = null,
    InfrastructureConfigId? ConfigId = null) : IQuery<IReadOnlyList<GitFileResult>>;
