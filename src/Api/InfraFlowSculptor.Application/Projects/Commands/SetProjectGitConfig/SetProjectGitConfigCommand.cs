using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectGitConfig;

/// <summary>Command to set or update the Git repository configuration on a project.</summary>
public record SetProjectGitConfigCommand(
    ProjectId ProjectId,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    string? BasePath,
    string? PipelineBasePath,
    string PersonalAccessToken
) : ICommand<Success>;
