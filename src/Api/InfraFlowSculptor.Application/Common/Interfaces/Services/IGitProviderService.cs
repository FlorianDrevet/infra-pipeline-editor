using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Abstracts Git hosting provider operations (push files, test connection).
/// </summary>
public interface IGitProviderService
{
    /// <summary>
    /// Tests the connection to the Git repository using the provided token.
    /// </summary>
    Task<ErrorOr<TestGitConnectionResult>> TestConnectionAsync(
        string token, string owner, string repositoryName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a branch and pushes files to the Git repository.
    /// </summary>
    Task<ErrorOr<PushBicepToGitResult>> PushFilesAsync(
        GitPushRequest request, CancellationToken cancellationToken = default);
}
