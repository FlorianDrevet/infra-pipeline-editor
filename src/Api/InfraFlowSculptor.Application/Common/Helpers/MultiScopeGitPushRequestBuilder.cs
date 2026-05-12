using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Builds <see cref="MultiScopeGitPushRequest"/> instances while merging generated files by repository scope.
/// </summary>
internal static class MultiScopeGitPushRequestBuilder
{
    /// <summary>
    /// Builds a multi-scope Git push request from the provided metadata and scoped generated files.
    /// </summary>
    /// <param name="token">The authentication token to use for the Git push.</param>
    /// <param name="owner">The repository owner.</param>
    /// <param name="repositoryName">The repository name.</param>
    /// <param name="baseBranch">The base branch to branch from.</param>
    /// <param name="targetBranchName">The branch name to create or update.</param>
    /// <param name="commitMessage">The commit message to use for the push.</param>
    /// <param name="scopes">The generated file scopes keyed by their optional base path.</param>
    /// <returns>
    /// The merged <see cref="MultiScopeGitPushRequest"/>, or an error when two scopes resolve to the same path with
    /// different content.
    /// </returns>
    internal static ErrorOr<MultiScopeGitPushRequest> Build(
        string token,
        string owner,
        string repositoryName,
        string baseBranch,
        string targetBranchName,
        string commitMessage,
        IReadOnlyList<(string? BasePath, IReadOnlyDictionary<string, string> Files)> scopes)
    {
        var mergedScopes = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        foreach (var (basePath, files) in scopes)
        {
            var mergeError = TryMergeFiles(mergedScopes, basePath, files);
            if (mergeError is not null)
                return mergeError.Value;
        }

        return new MultiScopeGitPushRequest
        {
            Token = token,
            Owner = owner,
            RepositoryName = repositoryName,
            BaseBranch = baseBranch,
            TargetBranchName = targetBranchName,
            CommitMessage = commitMessage,
            Scopes = mergedScopes
                .Select(scope => new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = string.IsNullOrEmpty(scope.Key) ? null : scope.Key,
                    Files = scope.Value,
                })
                .ToList(),
        };
    }

    private static Error? TryMergeFiles(
        IDictionary<string, Dictionary<string, string>> mergedScopes,
        string? basePath,
        IReadOnlyDictionary<string, string> files)
    {
        var normalizedBasePath = NormalizeBasePath(basePath);

        if (!string.IsNullOrEmpty(normalizedBasePath))
        {
            foreach (var (relativePath, content) in files)
            {
                var mergeError = TryAddScopedFile(mergedScopes, normalizedBasePath, relativePath, content);
                if (mergeError is not null)
                    return mergeError;
            }

            return null;
        }

        foreach (var (relativePath, content) in files)
        {
            // Root-level scopes are re-sliced by top-level folder so Git cleanup can delete stale generated
            // files without claiming the entire repository root as a cleanup scope.
            var (scopedBasePath, scopedRelativePath) = SplitRootScopedPath(relativePath);
            var mergeError = TryAddScopedFile(mergedScopes, scopedBasePath, scopedRelativePath, content);
            if (mergeError is not null)
            {
                return mergeError;
            }
        }

        return null;
    }

    private static Error? TryAddScopedFile(
        IDictionary<string, Dictionary<string, string>> mergedScopes,
        string basePath,
        string relativePath,
        string content)
    {
        if (!mergedScopes.TryGetValue(basePath, out var scopedFiles))
        {
            scopedFiles = new Dictionary<string, string>(StringComparer.Ordinal);
            mergedScopes[basePath] = scopedFiles;
        }

        if (scopedFiles.TryGetValue(relativePath, out var existingContent)
            && !string.Equals(existingContent, content, StringComparison.Ordinal))
        {
            var resolvedPath = CombinePath(basePath, relativePath);
            return Errors.GitRepository.PushFailed(
                $"Generated file collision detected for path '{resolvedPath}'.");
        }

        scopedFiles[relativePath] = content;
        return null;
    }

    private static (string BasePath, string RelativePath) SplitRootScopedPath(string relativePath)
    {
        var normalizedRelativePath = relativePath.TrimStart('/');
        var separatorIndex = normalizedRelativePath.IndexOf('/');

        return separatorIndex < 0
            ? (string.Empty, normalizedRelativePath)
            : (normalizedRelativePath[..separatorIndex], normalizedRelativePath[(separatorIndex + 1)..]);
    }

    private static string NormalizeBasePath(string? basePath) =>
        string.IsNullOrWhiteSpace(basePath)
            ? string.Empty
            : basePath.Trim('/');

    private static string CombinePath(string basePath, string relativePath) =>
        string.IsNullOrEmpty(basePath)
            ? relativePath.TrimStart('/')
            : $"{basePath}/{relativePath.TrimStart('/')}";
}