using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders;

/// <summary>
/// Pushes files to an Azure DevOps Git repository using the REST API.
/// Owner format: "{organization}/{project}".
/// </summary>
public sealed class AzureDevOpsGitProviderService(
    IHttpClientFactory httpClientFactory,
    ILogger<AzureDevOpsGitProviderService> logger)
    : IGitProviderService, IGitMultiScopePushProviderService
{
    private const string ApiVersion = "7.1";

    /// <inheritdoc />
    public async Task<ErrorOr<TestGitConnectionResult>> TestConnectionAsync(
        string token, string owner, string repositoryName, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient(token);
            var (org, project) = ParseOwner(owner);

            var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repositoryName}?api-version={ApiVersion}";
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var repo = await response.Content.ReadFromJsonAsync<AdoRepository>(cancellationToken: cancellationToken);
            var defaultBranch = repo?.DefaultBranch?.Replace("refs/heads/", "", StringComparison.Ordinal);

            return new TestGitConnectionResult(true, $"{org}/{project}/{repositoryName}", defaultBranch, null);
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

            using var client = CreateClient(request.Token);
            var (org, project) = ParseOwner(request.Owner);
            var repoApiBase = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}";

            // Resolve base branch SHA with EXACT ref name match (the ADO 'filter' parameter is a prefix match,
            // so 'heads/main' would also match 'mainline' — we must filter client-side by the exact name).
            var baseSha = await ResolveBranchShaAsync(client, repoApiBase, request.BaseBranch, cancellationToken);
            if (string.IsNullOrEmpty(baseSha))
                return Errors.GitRepository.PushFailed($"Base branch '{request.BaseBranch}' not found.");

            string? targetSha;
            bool targetBranchExists;
            if (string.Equals(request.TargetBranchName, request.BaseBranch, StringComparison.Ordinal))
            {
                targetSha = baseSha;
                targetBranchExists = true;
            }
            else
            {
                targetSha = await ResolveBranchShaAsync(client, repoApiBase, request.TargetBranchName, cancellationToken);
                targetBranchExists = !string.IsNullOrEmpty(targetSha);
            }

            // The push will be anchored on this exact commit. All existence checks MUST be performed
            // against this same commit to avoid race conditions and to guarantee that 'edit' changes
            // reference paths that actually exist at the parent commit.
            var parentSha = targetBranchExists ? targetSha! : baseSha;

            // ADO Git is case-sensitive on paths — use Ordinal comparisons throughout.
            var existingFilePaths = new HashSet<string>(StringComparer.Ordinal);
            var allExistingFilesInCleanupRoots = new HashSet<string>(StringComparer.Ordinal);

            foreach (var cleanupRoot in pushData.CleanupRoots)
            {
                var listed = await ListItemsAtCommitAsync(
                    client,
                    repoApiBase,
                    scopePath: $"/{cleanupRoot}",
                    commitSha: parentSha,
                    cancellationToken);

                foreach (var path in listed)
                    allExistingFilesInCleanupRoots.Add(path);
            }

            foreach (var filePath in pushData.FilesByPath.Keys)
            {
                if (allExistingFilesInCleanupRoots.Contains(filePath))
                {
                    existingFilePaths.Add(filePath);
                    continue;
                }

                if (IsWithinCleanupRoots(filePath, pushData.CleanupRoots))
                {
                    // File lives under a cleanup root we already enumerated → confirmed not present.
                    continue;
                }

                // Outside any cleanup root: probe individually at the exact parent commit.
                if (await ItemExistsAtCommitAsync(client, repoApiBase, filePath, parentSha, cancellationToken))
                    existingFilePaths.Add(filePath);
            }

            var changes = new List<object>(pushData.FilesByPath.Count + allExistingFilesInCleanupRoots.Count);
            foreach (var (filePath, content) in pushData.FilesByPath)
            {
                changes.Add(new
                {
                    changeType = existingFilePaths.Contains(filePath) ? "edit" : "add",
                    item = new { path = $"/{filePath}" },
                    newContent = new { content, contentType = "rawtext" },
                });
            }

            foreach (var existingFile in allExistingFilesInCleanupRoots)
            {
                if (pushData.FilesByPath.ContainsKey(existingFile))
                    continue;

                changes.Add(new
                {
                    changeType = "delete",
                    item = new { path = $"/{existingFile}" },
                });
            }

            if (changes.Count == 0)
            {
                // Nothing to commit (no new/changed files and nothing to delete).
                var emptyBranchUrl = $"https://dev.azure.com/{org}/{project}/_git/{request.RepositoryName}?version=GB{request.TargetBranchName}";
                return new PushBicepToGitResult(
                    request.TargetBranchName,
                    emptyBranchUrl,
                    parentSha,
                    0);
            }

            var pushPayload = new
            {
                refUpdates = new[]
                {
                    new
                    {
                        name = $"refs/heads/{request.TargetBranchName}",
                        oldObjectId = parentSha,
                    },
                },
                commits = new[]
                {
                    new
                    {
                        comment = request.CommitMessage,
                        changes,
                    },
                },
            };

            var pushUrl = $"{repoApiBase}/pushes?api-version={ApiVersion}";
            var pushResponse = await client.PostAsJsonAsync(pushUrl, pushPayload, cancellationToken);

            if (!pushResponse.IsSuccessStatusCode)
            {
                var body = await pushResponse.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "Azure DevOps push failed with {StatusCode}. ParentSha: {ParentSha}, target branch existed: {TargetBranchExists}, total changes: {ChangeCount}, files: {FileCount}, cleanup roots: [{CleanupRoots}]. Response: {Body}",
                    pushResponse.StatusCode,
                    parentSha,
                    targetBranchExists,
                    changes.Count,
                    pushData.FilesByPath.Count,
                    string.Join(", ", pushData.CleanupRoots),
                    body);
                return Errors.GitRepository.PushFailed($"ADO API returned {pushResponse.StatusCode}: {body}");
            }

            var pushResult = await pushResponse.Content.ReadFromJsonAsync<AdoPushResult>(cancellationToken: cancellationToken);
            var commitSha = pushResult?.Commits?.FirstOrDefault()?.CommitId ?? "unknown";

            var branchUrl = $"https://dev.azure.com/{org}/{project}/_git/{request.RepositoryName}?version=GB{request.TargetBranchName}";
            return new PushBicepToGitResult(
                request.TargetBranchName,
                branchUrl,
                commitSha,
                pushData.FilesByPath.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error pushing files to Azure DevOps repository '{Owner}/{Repo}'.", request.Owner, request.RepositoryName);
            return Errors.GitRepository.PushFailed(ex.Message);
        }
    }

    /// <summary>
    /// Resolves a branch tip SHA by EXACT ref name match.
    /// ADO's <c>refs?filter=heads/X</c> is a prefix match and would also return <c>heads/Xfoo</c>;
    /// we therefore filter client-side on <c>refs/heads/{branchName}</c>.
    /// </summary>
    private async Task<string?> ResolveBranchShaAsync(
        HttpClient client,
        string repoApiBase,
        string branchName,
        CancellationToken cancellationToken)
    {
        var url = $"{repoApiBase}/refs?filter=heads/{Uri.EscapeDataString(branchName)}&api-version={ApiVersion}";
        var response = await client.GetFromJsonAsync<AdoRefList>(url, cancellationToken);
        var fullName = $"refs/heads/{branchName}";
        return response?.Value?
            .FirstOrDefault(r => string.Equals(r.Name, fullName, StringComparison.Ordinal))?
            .ObjectId;
    }

    /// <summary>
    /// Recursively lists file paths under <paramref name="scopePath"/> at the exact <paramref name="commitSha"/>.
    /// Returns paths without the leading slash.
    /// </summary>
    private async Task<IReadOnlyCollection<string>> ListItemsAtCommitAsync(
        HttpClient client,
        string repoApiBase,
        string scopePath,
        string commitSha,
        CancellationToken cancellationToken)
    {
        var url = $"{repoApiBase}/items?scopePath={Uri.EscapeDataString(scopePath)}&recursionLevel=full&versionDescriptor.versionType=commit&versionDescriptor.version={commitSha}&api-version={ApiVersion}";
        var response = await client.GetAsync(url, cancellationToken);

        // Scope path missing at the parent commit → no existing files (this is normal for a fresh push).
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Array.Empty<string>();

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug(
                "Azure DevOps items listing returned {StatusCode} for scope '{ScopePath}' at commit {CommitSha}. Response: {Body}",
                response.StatusCode,
                scopePath,
                commitSha,
                body);
            return Array.Empty<string>();
        }

        var itemsList = await response.Content.ReadFromJsonAsync<AdoItemList>(cancellationToken: cancellationToken);
        if (itemsList?.Value is null)
            return Array.Empty<string>();

        var paths = new List<string>();
        foreach (var item in itemsList.Value)
        {
            if (item is { IsFolder: false, Path: not null })
                paths.Add(item.Path.TrimStart('/'));
        }

        return paths;
    }

    /// <summary>
    /// Checks whether a single file path exists at the exact <paramref name="commitSha"/>.
    /// </summary>
    private async Task<bool> ItemExistsAtCommitAsync(
        HttpClient client,
        string repoApiBase,
        string filePath,
        string commitSha,
        CancellationToken cancellationToken)
    {
        var url = $"{repoApiBase}/items?path=/{Uri.EscapeDataString(filePath)}&versionDescriptor.versionType=commit&versionDescriptor.version={commitSha}&api-version={ApiVersion}";
        var response = await client.GetAsync(url, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<GitBranchResult>>> ListBranchesAsync(
        string token, string owner, string repositoryName, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient(token);
            var (org, project) = ParseOwner(owner);

            var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repositoryName}/refs?filter=heads/&api-version={ApiVersion}";
            var response = await client.GetFromJsonAsync<AdoRefList>(url, cancellationToken);

            var results = (response?.Value ?? [])
                .Where(r => r.ObjectId is not null)
                .Select(r =>
                {
                    var name = r.Name?.Replace("refs/heads/", "", StringComparison.Ordinal) ?? "";
                    return new GitBranchResult(name, false);
                })
                .ToList();

            return results;
        }
        catch (Exception ex)
        {
            return Errors.GitRepository.ListBranchesFailed(ex.Message);
        }
    }

    private HttpClient CreateClient(string token)
    {
        var client = httpClientFactory.CreateClient();
        var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static (string Organization, string Project) ParseOwner(string owner)
    {
        var parts = owner.Split('/');
        return parts.Length >= 2
            ? (parts[0], parts[1])
            : (parts[0], parts[0]);
    }

    private static ErrorOr<PreparedAzureDevOpsPush> PrepareScopedPush(MultiScopeGitPushRequest request)
    {
        // ADO Git is case-sensitive on paths — keep Ordinal (case-sensitive) comparisons throughout the push pipeline.
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
            : new PreparedAzureDevOpsPush(filesByPath, cleanupRoots);
    }

    private static bool IsWithinCleanupRoots(string path, IReadOnlySet<string> cleanupRoots)
    {
        foreach (var cleanupRoot in cleanupRoots)
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

    private sealed record PreparedAzureDevOpsPush(
        IReadOnlyDictionary<string, string> FilesByPath,
        IReadOnlySet<string> CleanupRoots);

    // ─── ADO API response models ────────────────────────────────────────────

    private sealed class AdoRepository
    {
        [JsonPropertyName("defaultBranch")]
        public string? DefaultBranch { get; set; }
    }

    private sealed class AdoRefList
    {
        [JsonPropertyName("value")]
        public List<AdoRef>? Value { get; set; }
    }

    private sealed class AdoRef
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("objectId")]
        public string? ObjectId { get; set; }
    }

    private sealed class AdoPushResult
    {
        [JsonPropertyName("commits")]
        public List<AdoCommit>? Commits { get; set; }
    }

    private sealed class AdoCommit
    {
        [JsonPropertyName("commitId")]
        public string? CommitId { get; set; }
    }

    private sealed class AdoItemList
    {
        [JsonPropertyName("value")]
        public List<AdoItem>? Value { get; set; }
    }

    private sealed class AdoItem
    {
        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("isFolder")]
        public bool IsFolder { get; set; }
    }
}
