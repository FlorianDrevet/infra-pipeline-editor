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

            // 2. Create blobs for each file
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

            // 3. Create tree
            var newTree = new NewTree { BaseTree = baseSha };
            foreach (var item in treeItems)
                newTree.Tree.Add(item);

            var tree = await client.Git.Tree.Create(request.Owner, request.RepositoryName, newTree);

            // 4. Create commit
            var newCommit = new NewCommit(request.CommitMessage, tree.Sha, baseSha);
            var commit = await client.Git.Commit.Create(request.Owner, request.RepositoryName, newCommit);

            // 5. Create or update branch
            string branchRef = $"refs/heads/{request.TargetBranchName}";
            try
            {
                // Try to get existing branch
                var existingRef = await client.Git.Reference.Get(
                    request.Owner, request.RepositoryName, $"heads/{request.TargetBranchName}");
                // Branch exists — update it
                await client.Git.Reference.Update(
                    request.Owner, request.RepositoryName,
                    $"heads/{request.TargetBranchName}",
                    new ReferenceUpdate(commit.Sha));
            }
            catch (NotFoundException)
            {
                // Branch does not exist — create it
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

    private static GitHubClient CreateClient(string token)
    {
        var client = new GitHubClient(new ProductHeaderValue("InfraFlowSculptor"));
        client.Credentials = new Credentials(token);
        return client;
    }
}
