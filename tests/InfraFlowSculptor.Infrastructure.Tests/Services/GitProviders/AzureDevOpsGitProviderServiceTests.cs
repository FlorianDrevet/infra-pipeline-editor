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
                return CreateJsonResponse(new { value = Array.Empty<object>() });
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
        pushPayload.RootElement.GetProperty("commits")[0].GetProperty("changes").GetArrayLength()
            .Should().Be(1);
    }

    private static HttpResponseMessage CreateJsonResponse(object payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
    }

    private sealed record RecordedRequest(string Method, string RequestUri, string? Body);

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            responder(request);
    }
}
