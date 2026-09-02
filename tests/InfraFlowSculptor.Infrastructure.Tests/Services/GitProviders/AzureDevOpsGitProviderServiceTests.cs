using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Infrastructure.Services.GitProviders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services.GitProviders;

public sealed class AzureDevOpsGitProviderServiceTests
{
    [Fact]
    public async Task Given_CanceledToken_When_PushScopedFilesAsync_Then_PropagatesCancellationAsync()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: true);
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromCanceled<HttpResponseMessage>(cancellationToken));
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(new HttpClient(handler));
        var logger = Substitute.For<ILogger<AzureDevOpsGitProviderService>>();
        var sut = new AzureDevOpsGitProviderService(httpClientFactory, logger);
        var request = new MultiScopeGitPushRequest
        {
            Token = "ado-token",
            Owner = "my-org/my-project",
            RepositoryName = "infra-repo",
            BaseBranch = "main",
            TargetBranchName = "feature/generated",
            CommitMessage = "Push generated files",
            Scopes =
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "generated",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource storage {}",
                    },
                },
            ],
        };

        // Act
        Func<Task> act = async () => await sut.PushScopedFilesAsync(request, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Given_NewBranchAndScopedFile_When_PushScopedFilesAsync_Then_ReturnsCommitMetadataAsync()
    {
        // Arrange
        var recordedRequests = new List<RecordedRequest>();
        var handler = new StubHttpMessageHandler(async request =>
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();
            recordedRequests.Add(new RecordedRequest(request.Method.Method, request.RequestUri!.AbsoluteUri, body));

            var requestUri = request.RequestUri!.AbsoluteUri;
            if (requestUri.Contains("/refs?filter=heads/main", StringComparison.Ordinal))
            {
                return CreateJsonResponse(new
                {
                    value = new[]
                    {
                        new AzureDevOpsRefResponse("refs/heads/main", "base-sha"),
                    },
                });
            }

            if (requestUri.Contains("/refs?filter=heads/feature%2Fgenerated", StringComparison.Ordinal))
            {
                return CreateJsonResponse(new { value = Array.Empty<AzureDevOpsRefResponse>() });
            }

            if (requestUri.Contains("/items?scopePath=%2Fgenerated", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (requestUri.Contains("/pushes?api-version=7.1", StringComparison.Ordinal))
            {
                return CreateJsonResponse(new
                {
                    commits = new[]
                    {
                        new
                        {
                            commitId = "commit-sha",
                        },
                    },
                });
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Unexpected request: {requestUri}", Encoding.UTF8, "text/plain"),
            };
        });

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(new HttpClient(handler));

        var logger = Substitute.For<ILogger<AzureDevOpsGitProviderService>>();
        var sut = new AzureDevOpsGitProviderService(httpClientFactory, logger);

        var request = new MultiScopeGitPushRequest
        {
            Token = "ado-token",
            Owner = "my-org/my-project",
            RepositoryName = "infra-repo",
            BaseBranch = "main",
            TargetBranchName = "feature/generated",
            CommitMessage = "Push generated files",
            Scopes =
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "generated",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource storage 'Microsoft.Storage/storageAccounts@2024-01-01' = {}",
                    },
                },
            ],
        };

        // Act
        var result = await sut.PushScopedFilesAsync(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.BranchName.Should().Be("feature/generated");
        result.Value.BranchUrl.Should().Be("https://dev.azure.com/my-org/my-project/_git/infra-repo?version=GBfeature/generated");
        result.Value.CommitSha.Should().Be("commit-sha");
        result.Value.FileCount.Should().Be(1);

        var pushRequest = recordedRequests.Single(r => r.Method == HttpMethod.Post.Method);
        using var pushPayload = JsonDocument.Parse(pushRequest.Body!);
        pushPayload.RootElement.GetProperty("refUpdates")[0].GetProperty("name").GetString()
            .Should().Be("refs/heads/feature/generated");
        pushPayload.RootElement.GetProperty("refUpdates")[0].GetProperty("oldObjectId").GetString()
            .Should().Be("base-sha");
        var change = pushPayload.RootElement.GetProperty("commits")[0].GetProperty("changes")[0];
        change.GetProperty("changeType").GetString().Should().Be("add");
        change.GetProperty("item").GetProperty("path").GetString().Should().Be("/generated/main.bicep");
        change.GetProperty("newContent").GetProperty("content").GetString()
            .Should().Be("resource storage 'Microsoft.Storage/storageAccounts@2024-01-01' = {}");
        change.GetProperty("newContent").GetProperty("contentType").GetString().Should().Be("rawtext");
    }

    [Fact]
    public async Task Given_ExistingCleanupRoot_When_PushScopedFilesAsync_Then_EditIncludesContentAndDeleteOmitsContentAsync()
    {
        // Arrange
        var recordedRequests = new List<RecordedRequest>();
        var handler = new StubHttpMessageHandler(async request =>
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();
            recordedRequests.Add(new RecordedRequest(request.Method.Method, request.RequestUri!.AbsoluteUri, body));

            var requestUri = request.RequestUri!.AbsoluteUri;
            if (requestUri.Contains("/refs?filter=heads/main", StringComparison.Ordinal))
            {
                return CreateJsonResponse(new
                {
                    value = new[]
                    {
                        new
                        {
                            name = "refs/heads/main",
                            objectId = "base-sha",
                        },
                    },
                });
            }

            if (requestUri.Contains("/refs?filter=heads/feature%2Fgenerated", StringComparison.Ordinal))
            {
                return CreateJsonResponse(new
                {
                    value = new[]
                    {
                        new
                        {
                            name = "refs/heads/feature/generated",
                            objectId = "target-sha",
                        },
                    },
                });
            }

            if (requestUri.Contains("/items?scopePath=%2Fgenerated", StringComparison.Ordinal))
            {
                return CreateJsonResponse(new
                {
                    value = new[]
                    {
                        new
                        {
                            path = "/generated/main.bicep",
                            isFolder = false,
                        },
                        new
                        {
                            path = "/generated/stale.bicep",
                            isFolder = false,
                        },
                    },
                });
            }

            if (requestUri.Contains("/pushes?api-version=7.1", StringComparison.Ordinal))
            {
                return CreateJsonResponse(new
                {
                    commits = new[]
                    {
                        new
                        {
                            commitId = "commit-sha",
                        },
                    },
                });
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Unexpected request: {requestUri}", Encoding.UTF8, "text/plain"),
            };
        });

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(new HttpClient(handler));

        var logger = Substitute.For<ILogger<AzureDevOpsGitProviderService>>();
        var sut = new AzureDevOpsGitProviderService(httpClientFactory, logger);

        var request = new MultiScopeGitPushRequest
        {
            Token = "ado-token",
            Owner = "my-org/my-project",
            RepositoryName = "infra-repo",
            BaseBranch = "main",
            TargetBranchName = "feature/generated",
            CommitMessage = "Push generated files",
            Scopes =
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "generated",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "updated-content",
                    },
                },
            ],
        };

        // Act
        var result = await sut.PushScopedFilesAsync(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        var pushRequest = recordedRequests.Single(r => r.Method == HttpMethod.Post.Method);
        using var pushPayload = JsonDocument.Parse(pushRequest.Body!);
        var changes = pushPayload.RootElement.GetProperty("commits")[0].GetProperty("changes");
        changes.GetArrayLength().Should().Be(2);

        var editChange = changes.EnumerateArray()
            .Single(change => change.GetProperty("changeType").GetString() == "edit");
        editChange.GetProperty("item").GetProperty("path").GetString().Should().Be("/generated/main.bicep");
        editChange.GetProperty("newContent").GetProperty("content").GetString().Should().Be("updated-content");

        var deleteChange = changes.EnumerateArray()
            .Single(change => change.GetProperty("changeType").GetString() == "delete");
        deleteChange.GetProperty("item").GetProperty("path").GetString().Should().Be("/generated/stale.bicep");
        deleteChange.TryGetProperty("newContent", out _).Should().BeFalse();
    }

    private static HttpResponseMessage CreateJsonResponse(object payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
    }

    private sealed record RecordedRequest(string Method, string RequestUri, string? Body);

    private sealed record AzureDevOpsRefResponse(string name, string objectId);

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            responder(request);
    }
}
