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
public sealed class GitHubGitProviderService : IGitProviderService
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

            // 3. Create blobs for each file
            var treeItems = new List<NewTreeItem>();
            foreach (var (relativePath, content) in request.Files)
            {
                var blobPath = string.IsNullOrEmpty(request.BasePath)
                    ? relativePath
                    : $"{request.BasePath}/{relativePath}";

                treeItems.Add(new NewTreeItem
                {
                    Path = blobPath,
                    Mode = "100644",
                    Type = TreeType.Blob,
                    Content = content,
                });
            }

            // 4. Create tree based on the parent commit (target branch tip or base branch)
            var newTree = new NewTree { BaseTree = parentSha };
            foreach (var item in treeItems)
                newTree.Tree.Add(item);

            var tree = await client.Git.Tree.Create(request.Owner, request.RepositoryName, newTree);

            // 5. Create commit whose parent is the correct branch tip
            var newCommit = new NewCommit(request.CommitMessage, tree.Sha, parentSha);
            var commit = await client.Git.Commit.Create(request.Owner, request.RepositoryName, newCommit);

            // 6. Create or update branch reference
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
        var client = new GitHubClient(new ProductHeaderValue("InfraFlowSculptor"));
        client.Credentials = new Credentials(token);
        return client;
    }
}
