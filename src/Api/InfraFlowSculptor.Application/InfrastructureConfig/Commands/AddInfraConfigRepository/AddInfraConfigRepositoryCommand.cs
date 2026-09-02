using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;

/// <summary>Adds a Git repository to an InfrastructureConfig (MultiRepo project layout only).</summary>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="ConfigId">Identifier of the target infrastructure configuration.</param>
/// <param name="ProviderType">Git hosting provider type (<c>GitHub</c> or <c>AzureDevOps</c>).</param>
/// <param name="RepositoryUrl">Full repository URL.</param>
/// <param name="DefaultBranch">Default branch name (e.g. <c>main</c>).</param>
/// <param name="PersonalAccessToken">Transient PAT used to verify and save the repository.</param>
/// <param name="ContentKinds">List of content kinds hosted by the repository
/// (e.g. <c>Infrastructure</c>, <c>ApplicationCode</c>).</param>
public sealed record AddInfraConfigRepositoryCommand(
    ProjectId ProjectId,
    InfrastructureConfigId ConfigId,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    string? PersonalAccessToken,
    IReadOnlyList<string> ContentKinds) : IRequest<ErrorOr<InfraConfigRepositoryId>>;
