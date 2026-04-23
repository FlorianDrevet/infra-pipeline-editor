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

            var refsUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}/refs?filter=heads/{request.BaseBranch}&api-version={ApiVersion}";
            var refsResponse = await client.GetFromJsonAsync<AdoRefList>(refsUrl, cancellationToken);
            var baseSha = refsResponse?.Value?.FirstOrDefault()?.ObjectId;

            if (string.IsNullOrEmpty(baseSha))
                return Errors.GitRepository.PushFailed($"Base branch '{request.BaseBranch}' not found.");

            string? targetSha = null;
            var targetBranchExists = false;
            if (request.TargetBranchName != request.BaseBranch)
            {
                var targetRefsUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}/refs?filter=heads/{request.TargetBranchName}&api-version={ApiVersion}";
                var targetRefsResponse = await client.GetFromJsonAsync<AdoRefList>(targetRefsUrl, cancellationToken);
                targetSha = targetRefsResponse?.Value?.FirstOrDefault()?.ObjectId;
                targetBranchExists = !string.IsNullOrEmpty(targetSha);
            }
            else
            {
                targetSha = baseSha;
                targetBranchExists = true;
            }

            var branchToInspect = targetSha is not null ? request.TargetBranchName : request.BaseBranch;
            var existingFilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allExistingFilesInCleanupRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var cleanupRoot in pushData.CleanupRoots)
            {
                var targetDirPath = $"/{cleanupRoot}";
                var itemsUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}/items?scopePath={targetDirPath}&recursionLevel=full&versionDescriptor.version={branchToInspect}&api-version={ApiVersion}";
                var itemsResponse = await client.GetAsync(itemsUrl, cancellationToken);
                if (!itemsResponse.IsSuccessStatusCode)
                    continue;

                var itemsList = await itemsResponse.Content.ReadFromJsonAsync<AdoItemList>(cancellationToken: cancellationToken);
                if (itemsList?.Value is null)
                    continue;

                foreach (var item in itemsList.Value)
                {
                    if (item is { IsFolder: false, Path: not null })
                        allExistingFilesInCleanupRoots.Add(item.Path.TrimStart('/'));
                }
            }

            foreach (var filePath in pushData.FilesByPath.Keys)
            {
                if (allExistingFilesInCleanupRoots.Contains(filePath))
                {
                    existingFilePaths.Add(filePath);
                    continue;
                }

                if (IsWithinCleanupRoots(filePath, pushData.CleanupRoots))
                    continue;

                var itemUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}/items?path=/{filePath}&versionDescriptor.version={branchToInspect}&api-version={ApiVersion}";
                var itemResponse = await client.GetAsync(itemUrl, cancellationToken);
                if (itemResponse.IsSuccessStatusCode)
                    existingFilePaths.Add(filePath);
            }

            var changes = new List<object>();
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

            var refUpdates = new List<object>
            {
                new
                {
                    name = $"refs/heads/{request.TargetBranchName}",
                    // ADO expects a new branch push with edit/delete changes to be anchored on the base commit,
                    // not on the empty-tree sentinel object id.
                    oldObjectId = targetBranchExists ? targetSha : baseSha,
                },
            };

            var pushPayload = new
            {
                refUpdates,
                commits = new[]
                {
                    new
                    {
                        comment = request.CommitMessage,
                        changes,
                    },
                },
            };

            var pushUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}/pushes?api-version={ApiVersion}";
            var pushResponse = await client.PostAsJsonAsync(pushUrl, pushPayload, cancellationToken);

            if (!pushResponse.IsSuccessStatusCode)
            {
                var body = await pushResponse.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "Azure DevOps push failed with {StatusCode}. Total changes: {ChangeCount}, files: {FileCount}, cleanup roots: [{CleanupRoots}]. Response: {Body}",
                    pushResponse.StatusCode,
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
            return Errors.GitRepository.PushFailed(ex.Message);
        }
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
        var filesByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cleanupRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
            if (path.StartsWith($"{cleanupRoot}/", StringComparison.OrdinalIgnoreCase))
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
