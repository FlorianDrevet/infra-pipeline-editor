using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders;

/// <summary>
/// Resolves the correct <see cref="IGitProviderService"/> based on <see cref="GitProviderType"/>.
/// </summary>
public sealed class GitProviderFactory(
    GitHubGitProviderService gitHub,
    AzureDevOpsGitProviderService azureDevOps)
    : IGitProviderFactory
{
    /// <inheritdoc />
    public IGitProviderService Create(GitProviderType providerType) =>
        providerType.Value switch
        {
            GitProviderTypeEnum.GitHub => gitHub,
            GitProviderTypeEnum.AzureDevOps => azureDevOps,
            _ => throw new ArgumentOutOfRangeException(nameof(providerType), providerType, "Unknown Git provider type."),
        };
}
