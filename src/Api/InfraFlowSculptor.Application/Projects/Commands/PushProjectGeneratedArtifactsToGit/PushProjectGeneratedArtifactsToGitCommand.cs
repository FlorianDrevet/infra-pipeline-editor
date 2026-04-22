using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectGeneratedArtifactsToGit;

/// <summary>
/// Requests a single Git commit containing the latest project-level generated Bicep, pipeline, and bootstrap artifacts.
/// </summary>
/// <param name="ProjectId">The identifier of the project to push.</param>
/// <param name="BranchName">The target branch name to create or update.</param>
/// <param name="CommitMessage">The Git commit message.</param>
public sealed record PushProjectGeneratedArtifactsToGitCommand(
    ProjectId ProjectId,
    string BranchName,
    string CommitMessage) : ICommand<PushBicepToGitResult>;