using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Common.Services;

/// <summary>
/// Resolves the correct Git provider and executes a multi-scope push when the provider supports it.
/// </summary>
public sealed class MultiScopeGitPushExecutor(IGitProviderFactory gitProviderFactory) : IMultiScopeGitPushExecutor
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushBicepToGitResult>> PushAsync(
        ResolvedRepositoryTarget target,
        MultiScopeGitPushRequest request,
        string unsupportedCapabilityReason,
        CancellationToken cancellationToken = default)
    {
        var provider = gitProviderFactory.Create(target.ProviderType);
        if (provider is not IGitMultiScopePushProviderService multiScopeProvider)
            return Errors.GitRepository.PushFailed(unsupportedCapabilityReason);

        return await multiScopeProvider.PushScopedFilesAsync(request, cancellationToken);
    }
}