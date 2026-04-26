using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoFiles;

/// <summary>Searches for files matching a filename pattern in the application-code Git repository.</summary>
public record SearchCodeRepoFilesQuery(
    ProjectId ProjectId,
    string Branch,
    string? FilenamePattern = null,
    InfrastructureConfigId? ConfigId = null) : IQuery<IReadOnlyList<GitFileResult>>;
