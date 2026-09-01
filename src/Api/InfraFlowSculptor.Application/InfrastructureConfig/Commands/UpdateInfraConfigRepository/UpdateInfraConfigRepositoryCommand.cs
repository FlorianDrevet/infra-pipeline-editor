using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateInfraConfigRepository;

/// <summary>Updates an existing InfraConfigRepository.</summary>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="ConfigId">Identifier of the target infrastructure configuration.</param>
/// <param name="RepositoryId">Identifier of the repository entity to update.</param>
/// <param name="ProviderType">Git hosting provider type (<c>GitHub</c> or <c>AzureDevOps</c>).</param>
/// <param name="RepositoryUrl">Full repository URL.</param>
/// <param name="DefaultBranch">Default branch name (e.g. <c>main</c>).</param>
/// <param name="PersonalAccessToken">Optional transient PAT. When omitted, the existing repository-scoped PAT is reused.</param>
/// <param name="ContentKinds">List of content kinds hosted by the repository.</param>
public sealed record UpdateInfraConfigRepositoryCommand(
    ProjectId ProjectId,
    InfrastructureConfigId ConfigId,
    InfraConfigRepositoryId RepositoryId,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    string? PersonalAccessToken,
    IReadOnlyList<string> ContentKinds) : IRequest<ErrorOr<Updated>>;
