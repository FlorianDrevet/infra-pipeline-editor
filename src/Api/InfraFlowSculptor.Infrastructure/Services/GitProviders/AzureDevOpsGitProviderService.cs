using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders;

/// <summary>
/// Pushes files to an Azure DevOps Git repository using the REST API.
/// Owner format: "{organization}/{project}".
/// </summary>
public sealed class AzureDevOpsGitProviderService(IHttpClientFactory httpClientFactory) : IGitProviderService
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
    public async Task<ErrorOr<PushBicepToGitResult>> PushFilesAsync(
        GitPushRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient(request.Token);
            var (org, project) = ParseOwner(request.Owner);

            // 1. Get the base branch SHA
            var refsUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}/refs?filter=heads/{request.BaseBranch}&api-version={ApiVersion}";
            var refsResponse = await client.GetFromJsonAsync<AdoRefList>(refsUrl, cancellationToken);
            var baseSha = refsResponse?.Value?.FirstOrDefault()?.ObjectId;

            if (string.IsNullOrEmpty(baseSha))
                return Errors.GitRepository.PushFailed($"Base branch '{request.BaseBranch}' not found.");

            // 2. Check if target branch exists
            string? targetSha = null;
            if (request.TargetBranchName != request.BaseBranch)
            {
                var targetRefsUrl = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{request.RepositoryName}/refs?filter=heads/{request.TargetBranchName}&api-version={ApiVersion}";
                var targetRefsResponse = await client.GetFromJsonAsync<AdoRefList>(targetRefsUrl, cancellationToken);
                targetSha = targetRefsResponse?.Value?.FirstOrDefault()?.ObjectId;
            }

            // 3. Build the push payload
            var changes = new List<object>();
            foreach (var (relativePath, content) in request.Files)
            {
                var filePath = string.IsNullOrEmpty(request.BasePath)
                    ? relativePath
                    : $"{request.BasePath}/{relativePath}";

                // Use "add" for new files — ADO will auto-handle "edit" if the file exists
                changes.Add(new
                {
                    changeType = "add",
                    item = new { path = $"/{filePath}" },
                    newContent = new { content, contentType = "rawtext" },
                });
            }

            var refUpdates = new List<object>();
            if (targetSha is not null)
            {
                // Branch exists — update
                refUpdates.Add(new
                {
                    name = $"refs/heads/{request.TargetBranchName}",
                    oldObjectId = targetSha,
                });
            }
            else
            {
                // Branch does not exist — create from base
                refUpdates.Add(new
                {
                    name = $"refs/heads/{request.TargetBranchName}",
                    oldObjectId = "0000000000000000000000000000000000000000",
                });
            }

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
                return Errors.GitRepository.PushFailed($"ADO API returned {pushResponse.StatusCode}: {body}");
            }

            var pushResult = await pushResponse.Content.ReadFromJsonAsync<AdoPushResult>(cancellationToken: cancellationToken);
            var commitSha = pushResult?.Commits?.FirstOrDefault()?.CommitId ?? "unknown";

            var branchUrl = $"https://dev.azure.com/{org}/{project}/_git/{request.RepositoryName}?version=GB{request.TargetBranchName}";
            return new PushBicepToGitResult(request.TargetBranchName, branchUrl, commitSha, request.Files.Count);
        }
        catch (Exception ex)
        {
            return Errors.GitRepository.PushFailed(ex.Message);
        }
    }

    private static HttpClient CreateClient(string token)
    {
        var client = new HttpClient();
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
}
