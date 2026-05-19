using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

internal static class ProjectRepositoryConnectionVerifier
{
    internal static bool HasCompleteConnectionDetails(
        GitProviderType? providerType,
        string? repositoryUrl,
        string? defaultBranch)
    {
        return providerType is not null
            && !string.IsNullOrWhiteSpace(repositoryUrl)
            && !string.IsNullOrWhiteSpace(defaultBranch);
    }

    internal static async Task<ErrorOr<ProjectRepositoryBranchVerification>> VerifyBranchesAsync(
        IGitProviderFactory gitProviderFactory,
        GitProviderType providerType,
        string repositoryUrl,
        string personalAccessToken,
        string? requiredDefaultBranch,
        CancellationToken cancellationToken)
    {
        (string owner, string repositoryName) parsed;
        try
        {
            parsed = RepositoryUrlHelper.ExtractOwnerAndRepo(providerType, repositoryUrl);
        }
        catch (UriFormatException)
        {
            return Errors.ProjectRepository.InvalidUrl(repositoryUrl);
        }

        var gitProvider = gitProviderFactory.Create(providerType);
        var branchesResult = await gitProvider.ListBranchesAsync(
            personalAccessToken,
            parsed.owner,
            parsed.repositoryName,
            cancellationToken);
        if (branchesResult.IsError)
        {
            return branchesResult.Errors;
        }

        if (!string.IsNullOrWhiteSpace(requiredDefaultBranch)
            && !branchesResult.Value.Any(branch => string.Equals(branch.Name, requiredDefaultBranch, StringComparison.OrdinalIgnoreCase)))
        {
            return Errors.ProjectRepository.DefaultBranchNotFound(requiredDefaultBranch);
        }

        return new ProjectRepositoryBranchVerification(parsed.owner, parsed.repositoryName, branchesResult.Value);
    }
}