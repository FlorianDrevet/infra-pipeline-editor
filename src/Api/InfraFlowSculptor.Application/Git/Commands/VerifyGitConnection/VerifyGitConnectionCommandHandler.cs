using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Git.Commands.VerifyGitConnection;

/// <summary>Handles the <see cref="VerifyGitConnectionCommand"/>.</summary>
/// <remarks>
/// Verifies the Git connection without requiring a project context.
/// Parses the provider type, calls the verifier, and returns the branch list with a default candidate.
/// </remarks>
public sealed class VerifyGitConnectionCommandHandler(
    IGitProviderFactory gitProviderFactory)
    : ICommandHandler<VerifyGitConnectionCommand, ProjectRepositoryConnectionVerificationResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectRepositoryConnectionVerificationResult>> Handle(
        VerifyGitConnectionCommand command,
        CancellationToken cancellationToken)
    {
        var providerTypeResult = EnumValueObjectParser.Parse<GitProviderTypeEnum, GitProviderType>(
            command.ProviderType,
            static parsed => new GitProviderType(parsed),
            Errors.GitRepository.InvalidProviderType);
        if (providerTypeResult.IsError)
            return providerTypeResult.Errors;

        var branchesResult = await ProjectRepositoryConnectionVerifier.VerifyBranchesAsync(
            gitProviderFactory,
            providerTypeResult.Value,
            command.RepositoryUrl,
            command.PersonalAccessToken,
            requiredDefaultBranch: null,
            cancellationToken);
        if (branchesResult.IsError)
            return branchesResult.Errors;

        return new ProjectRepositoryConnectionVerificationResult(
            branchesResult.Value.Owner,
            branchesResult.Value.RepositoryName,
            branchesResult.Value.Branches,
            GetDefaultBranchCandidate(branchesResult.Value.Branches));
    }

    private static string? GetDefaultBranchCandidate(IReadOnlyList<GitBranchResult> branches)
    {
        return branches.FirstOrDefault(branch => string.Equals(branch.Name, "main", StringComparison.OrdinalIgnoreCase))?.Name
            ?? branches.FirstOrDefault()?.Name;
    }
}
