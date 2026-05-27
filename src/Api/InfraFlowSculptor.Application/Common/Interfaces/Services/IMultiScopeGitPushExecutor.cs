using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Executes Git pushes that update multiple repository roots in a single commit.
/// </summary>
public interface IMultiScopeGitPushExecutor
{
    /// <summary>
    /// Pushes the provided multi-scope request through the Git provider resolved from the target repository.
    /// </summary>
    /// <param name="target">The resolved repository target that defines the provider and repository metadata.</param>
    /// <param name="request">The already-built multi-scope push request.</param>
    /// <param name="unsupportedCapabilityReason">
    /// The reason to expose when the selected provider does not implement
    /// <see cref="IGitMultiScopePushProviderService"/>.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The Git push result, or an error when the provider cannot execute the request.</returns>
    Task<ErrorOr<PushBicepToGitResult>> PushAsync(
        ResolvedRepositoryTarget target,
        MultiScopeGitPushRequest request,
        string unsupportedCapabilityReason,
        CancellationToken cancellationToken = default);
}
