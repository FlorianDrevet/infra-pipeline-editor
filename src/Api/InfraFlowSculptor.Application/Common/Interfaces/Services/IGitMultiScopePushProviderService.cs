using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Provides Git push operations that can update multiple repository roots in a single commit.
/// </summary>
public interface IGitMultiScopePushProviderService
{
    /// <summary>
    /// Creates or updates a branch and pushes files from multiple repository roots in a single commit.
    /// </summary>
    /// <param name="request">The Git push request describing the repository metadata and scoped files to push.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The result of the Git push operation.</returns>
    Task<ErrorOr<PushBicepToGitResult>> PushScopedFilesAsync(
        MultiScopeGitPushRequest request,
        CancellationToken cancellationToken = default);
}