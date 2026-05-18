using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectGitPat;

/// <summary>
/// Command to store or update a repository-scoped Git personal access token.
/// </summary>
public record SetProjectGitPatCommand(
    ProjectId ProjectId,
    ProjectRepositoryId RepositoryId,
    string PersonalAccessToken) : ICommand<Success>;