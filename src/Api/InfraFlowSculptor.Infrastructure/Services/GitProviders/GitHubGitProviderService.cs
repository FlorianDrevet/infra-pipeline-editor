using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using Octokit;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders;

/// <summary>
/// Pushes files to a GitHub repository using the Octokit SDK.
/// Supports creating new branches and updating existing ones.
/// </summary>
public sealed class GitHubGitProviderService(IGitHubTreeApi gitHubTreeApi)
    : IGitProviderService, IGitMultiScopePushProviderService
{

    /// <inheritdoc />
    public async Task<ErrorOr<TestGitConnectionResult>> TestConnectionAsync(
        string token, string owner, string repositoryName, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(token);
            var repo = await client.Repository.Get(owner, repositoryName);
            return new TestGitConnectionResult(true, repo.FullName, repo.DefaultBranch, null);
        }
        catch (Exception ex)
        {
            return new TestGitConnectionResult(false, null, null, ex.Message);
        }
    }

    /// <inheritdoc />
    public Task<ErrorOr<PushBicepToGitResult>> PushFilesAsync(
        GitPushRequest request,
        CancellationToken cancellationToken = default) =>
        PushScopedFilesAsync(
            new MultiScopeGitPushRequest
            {
                Token = request.Token,
                Owner = request.Owner,
                RepositoryName = request.RepositoryName,
                BaseBranch = request.BaseBranch,
                TargetBranchName = request.TargetBranchName,
                CommitMessage = request.CommitMessage,
                Scopes =
                [
                    new MultiScopeGitPushRequest.GitPushScope
                    {
                        BasePath = request.BasePath,
                        Files = request.Files,
                    },
                ],
            },
            cancellationToken);

    /// <inheritdoc />
    public async Task<ErrorOr<PushBicepToGitResult>> PushScopedFilesAsync(
        MultiScopeGitPushRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preparedPush = PrepareScopedPush(request);
            if (preparedPush.IsError)
                return preparedPush.Errors;

            var pushData = preparedPush.Value;
            var client = CreateClient(request.Token);

            var baseBranchRef = await client.Git.Reference.Get(
                request.Owner,
                request.RepositoryName,
                $"heads/{request.BaseBranch}");
            var baseSha = baseBranchRef.Object.Sha;

            string parentSha = baseSha;
            bool targetBranchExists = false;
            try
            {
                var existingRef = await client.Git.Reference.Get(
                    request.Owner,
                    request.RepositoryName,
                    $"heads/{request.TargetBranchName}");
                parentSha = existingRef.Object.Sha;
                targetBranchExists = true;
            }
            catch (NotFoundException)
            {
                // Target branch does not exist yet — use the base branch SHA.
            }

            var treeItems = pushData.FilesByPath
                .Select(file => (object)new
                {
                    Path = file.Key,
                    Mode = "100644",
                    Type = "blob",
                    Content = file.Value,
                })
                .ToList();

            var parentCommit = await client.Git.Commit.Get(request.Owner, request.RepositoryName, parentSha);
            var parentTreeSha = parentCommit.Tree.Sha;

            if (pushData.CleanupRoots.Count > 0)
            {
                var existingTree = await client.Git.Tree.GetRecursive(
                    request.Owner,
                    request.RepositoryName,
                    parentTreeSha);

                foreach (var item in existingTree.Tree)
                {
                    if (item.Type != TreeType.Blob || string.IsNullOrEmpty(item.Path))
                        continue;

                    if (ShouldDeleteFile(item.Path, pushData))
                    {
                        treeItems.Add(new
                        {
                            Path = item.Path,
                            Mode = "100644",
                            Type = "blob",
                            Sha = (string?)null,
                        });
                    }
                }
            }

            var treeSha = await CreateTreeAsync(
                request.Token,
                request.Owner,
                request.RepositoryName,
                parentTreeSha,
                treeItems,
                cancellationToken);

            var newCommit = new NewCommit(request.CommitMessage, treeSha, parentSha);
            var commit = await client.Git.Commit.Create(request.Owner, request.RepositoryName, newCommit);

            string branchRef = $"refs/heads/{request.TargetBranchName}";
            if (targetBranchExists)
            {
                await client.Git.Reference.Update(
                    request.Owner,
                    request.RepositoryName,
                    $"heads/{request.TargetBranchName}",
                    new ReferenceUpdate(commit.Sha));
            }
            else
            {
                await client.Git.Reference.Create(
                    request.Owner,
                    request.RepositoryName,
                    new NewReference(branchRef, commit.Sha));
            }

            var branchUrl = $"https://github.com/{request.Owner}/{request.RepositoryName}/tree/{request.TargetBranchName}";
            return new PushBicepToGitResult(
                request.TargetBranchName,
                branchUrl,
                commit.Sha,
                pushData.FilesByPath.Count);
        }
        catch (Exception ex)
        {
            return Errors.GitRepository.PushFailed(ex.Message);
        }
    }

    private async Task<string> CreateTreeAsync(
        string token,
        string owner,
        string repositoryName,
        string baseTreeSha,
        IReadOnlyList<object> treeItems,
        CancellationToken cancellationToken)
    {
        var response = await gitHubTreeApi.CreateTreeAsync(
            owner,
            repositoryName,
            new { base_tree = baseTreeSha, tree = treeItems },
            token,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(response.Sha))
            throw new InvalidOperationException("GitHub API did not return the created tree SHA.");

        return response.Sha;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<GitBranchResult>>> ListBranchesAsync(
        string token, string owner, string repositoryName, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(token);
            var branches = await client.Repository.Branch.GetAll(owner, repositoryName);

            var results = branches
                .Select(b => new GitBranchResult(b.Name, b.Protected))
                .ToList();

            return results;
        }
        catch (Exception ex)
        {
            return Errors.GitRepository.ListBranchesFailed(ex.Message);
        }
    }

    private static GitHubClient CreateClient(string token)
    {
        var client = new GitHubClient(new Octokit.ProductHeaderValue("InfraFlowSculptor"));
        client.Credentials = new Credentials(token);
        return client;
    }

    private static ErrorOr<PreparedGitHubPush> PrepareScopedPush(MultiScopeGitPushRequest request)
    {
        var filesByPath = new Dictionary<string, string>(StringComparer.Ordinal);
        var cleanupRoots = new HashSet<string>(StringComparer.Ordinal);

        foreach (var scope in request.Scopes)
        {
            var normalizedBasePath = NormalizeBasePath(scope.BasePath);
            if (!string.IsNullOrEmpty(normalizedBasePath))
                cleanupRoots.Add(normalizedBasePath);

            foreach (var (relativePath, content) in scope.Files)
            {
                var fullPath = CombinePath(normalizedBasePath, relativePath);
                if (filesByPath.TryGetValue(fullPath, out var existingContent)
                    && !string.Equals(existingContent, content, StringComparison.Ordinal))
                {
                    return Errors.GitRepository.PushFailed(
                        $"Generated file collision detected for path '{fullPath}'.");
                }

                filesByPath[fullPath] = content;
            }
        }

        return filesByPath.Count == 0
            ? Errors.GitRepository.PushFailed("No generated files were provided for the Git push.")
            : new PreparedGitHubPush(filesByPath, cleanupRoots);
    }

    private static bool ShouldDeleteFile(string path, PreparedGitHubPush pushData)
    {
        if (pushData.FilesByPath.ContainsKey(path))
            return false;

        foreach (var cleanupRoot in pushData.CleanupRoots)
        {
            if (path.StartsWith($"{cleanupRoot}/", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string NormalizeBasePath(string? basePath) =>
        string.IsNullOrWhiteSpace(basePath)
            ? string.Empty
            : basePath.Trim('/');

    private static string CombinePath(string basePath, string relativePath) =>
        string.IsNullOrEmpty(basePath)
            ? relativePath.TrimStart('/')
            : $"{basePath}/{relativePath.TrimStart('/')}";

    private sealed record PreparedGitHubPush(
        IReadOnlyDictionary<string, string> FilesByPath,
        IReadOnlySet<string> CleanupRoots);
}
