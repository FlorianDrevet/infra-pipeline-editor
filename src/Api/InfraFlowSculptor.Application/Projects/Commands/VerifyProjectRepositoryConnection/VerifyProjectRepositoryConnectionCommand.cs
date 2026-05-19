using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.VerifyProjectRepositoryConnection;

/// <summary>Command to verify a project repository connection and list branches without saving changes.</summary>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="RepositoryId">Optional existing repository id when verifying an edit.</param>
/// <param name="ProviderType">Git hosting provider type.</param>
/// <param name="RepositoryUrl">Repository URL to verify.</param>
/// <param name="PersonalAccessToken">Optional transient PAT. Required for create verification.</param>
public sealed record VerifyProjectRepositoryConnectionCommand(
    ProjectId ProjectId,
    ProjectRepositoryId? RepositoryId,
    string ProviderType,
    string RepositoryUrl,
    string? PersonalAccessToken)
    : ICommand<ProjectRepositoryConnectionVerificationResult>;