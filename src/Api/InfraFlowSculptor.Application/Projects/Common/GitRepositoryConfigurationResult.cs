using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result representing a Git repository configuration.</summary>
public record GitRepositoryConfigurationResult(
    GitRepositoryConfigurationId Id,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    string? BasePath,
    string? PipelineBasePath,
    string Owner,
    string RepositoryName);
