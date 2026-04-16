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
public sealed class GitHubGitProviderService(IGitHubTreeApi gitHubTreeApi) : IGitProviderService
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
    public async Task<ErrorOr<PushBicepToGitResult>> PushFilesAsync(
        GitPushRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(request.Token);

            // 1. Get the base branch SHA
            var baseBranchRef = await client.Git.Reference.Get(
                request.Owner, request.RepositoryName, $"heads/{request.BaseBranch}");
            var baseSha = baseBranchRef.Object.Sha;

            // 2. Determine parent SHA: use the target branch tip when it already exists
            string parentSha = baseSha;
            bool targetBranchExists = false;
            try
            {
                var existingRef = await client.Git.Reference.Get(
                    request.Owner, request.RepositoryName, $"heads/{request.TargetBranchName}");
                parentSha = existingRef.Object.Sha;
                targetBranchExists = true;
            }
            catch (NotFoundException)
            {
                // Target branch does not exist yet — use the base branch SHA
            }

            // 3. Determine the target directory prefix for cleanup
            var targetPrefix = string.IsNullOrEmpty(request.BasePath) ? "" : $"{request.BasePath}/";

            // 4. Build the target tree payload and track generated files.
            var newFilePaths = new HashSet<string>(StringComparer.Ordinal);
            var treeItems = new List<object>();

            foreach (var (relativePath, content) in request.Files)
            {
                var blobPath = string.IsNullOrEmpty(request.BasePath)
                    ? relativePath
                    : $"{request.BasePath}/{relativePath}";

                newFilePaths.Add(blobPath);

                treeItems.Add(new
                {
                    Path = blobPath,
                    Mode = "100644",
                    Type = "blob",
                    Content = content,
                });
            }

            // 5. Get recursive tree of the parent commit to find stale files to delete
            var parentCommit = await client.Git.Commit.Get(request.Owner, request.RepositoryName, parentSha);
            var parentTreeSha = parentCommit.Tree.Sha;

            // Only clean up stale files when a BasePath is configured,
            // otherwise we'd delete every other file in the repository.
            if (!string.IsNullOrEmpty(targetPrefix))
            {
                var existingTree = await client.Git.Tree.GetRecursive(
                    request.Owner, request.RepositoryName, parentTreeSha);

                foreach (var item in existingTree.Tree)
                {
                    if (item.Type != TreeType.Blob)
                        continue;

                    if (item.Path.StartsWith(targetPrefix, StringComparison.Ordinal)
                        && !newFilePaths.Contains(item.Path))
                    {
                        // GitHub expects explicit "sha": null for deletions.
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

            // 6. Create tree with a direct HTTP call so null sha values are preserved.
            var treeSha = await CreateTreeAsync(
                request.Token,
                request.Owner,
                request.RepositoryName,
                parentTreeSha,
                treeItems,
                cancellationToken);

            // 7. Create commit whose parent is the correct branch tip
            var newCommit = new NewCommit(request.CommitMessage, treeSha, parentSha);
            var commit = await client.Git.Commit.Create(request.Owner, request.RepositoryName, newCommit);

            // 8. Create or update branch reference
            string branchRef = $"refs/heads/{request.TargetBranchName}";
            if (targetBranchExists)
            {
                await client.Git.Reference.Update(
                    request.Owner, request.RepositoryName,
                    $"heads/{request.TargetBranchName}",
                    new ReferenceUpdate(commit.Sha));
            }
            else
            {
                await client.Git.Reference.Create(
                    request.Owner, request.RepositoryName,
                    new NewReference(branchRef, commit.Sha));
            }

            var branchUrl = $"https://github.com/{request.Owner}/{request.RepositoryName}/tree/{request.TargetBranchName}";
            return new PushBicepToGitResult(request.TargetBranchName, branchUrl, commit.Sha, request.Files.Count);
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
        IReadOnlyCollection<object> treeItems,
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
    public async Task<ErrorOr<IReadOnlyCollection<GitBranchResult>>> ListBranchesAsync(
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
}
