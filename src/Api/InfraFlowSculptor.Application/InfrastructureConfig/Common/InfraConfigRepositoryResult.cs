namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>Application-layer result describing a Git repository declared on an <see cref="Domain.InfrastructureConfigAggregate.InfrastructureConfig"/> when the parent project layout is MultiRepo.</summary>
public sealed record InfraConfigRepositoryResult(
    Guid Id,
    string Alias,
    string ProviderType,
    string RepositoryUrl,
    string Owner,
    string RepositoryName,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds);
