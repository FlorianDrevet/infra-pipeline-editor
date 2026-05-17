using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Infrastructure.Services.GitProviders;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services.GitProviders;

public sealed class GitHubGitProviderServiceTests
{
    private readonly IGitHubTreeApi _gitHubTreeApi = Substitute.For<IGitHubTreeApi>();
    private readonly GitHubGitProviderService _sut;

    public GitHubGitProviderServiceTests()
    {
        _sut = new GitHubGitProviderService(_gitHubTreeApi);
    }

    // ──────────────────────────────────────────────────
    //  PrepareScopedPush — collision detection
    //  (validated before any GitHub API call)
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_NoFiles_When_PushScopedFilesAsync_Then_ReturnsErrorAsync()
    {
        // Arrange
        var request = CreateMultiScopeRequest(
            scopes:
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra",
                    Files = new Dictionary<string, string>(),
                },
            ]);

        // Act
        var result = await _sut.PushScopedFilesAsync(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Contain("No generated files were provided");
    }

    [Fact]
    public async Task Given_DuplicatePathWithDifferentContent_When_PushScopedFilesAsync_Then_ReturnsCollisionErrorAsync()
    {
        // Arrange
        var request = CreateMultiScopeRequest(
            scopes:
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource a {}",
                    },
                },
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource b {}",
                    },
                },
            ]);

        // Act
        var result = await _sut.PushScopedFilesAsync(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Contain("collision");
        result.FirstError.Description.Should().Contain("infra/main.bicep");
    }

    [Fact]
    public async Task Given_DuplicatePathWithSameContent_When_PushScopedFilesAsync_Then_DoesNotReturnCollisionErrorAsync()
    {
        // Arrange — same path, same content → no collision, but will fail on API call
        var request = CreateMultiScopeRequest(
            scopes:
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource a {}",
                    },
                },
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource a {}",
                    },
                },
            ]);

        // Act
        var result = await _sut.PushScopedFilesAsync(request);

        // Assert — passes validation but fails on API (no real token)
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().NotContain("collision");
    }

    [Fact]
    public async Task Given_BasePathWithLeadingAndTrailingSlashes_When_PushScopedFilesAsync_Then_NormalizesPathAsync()
    {
        // Arrange — basePath with slashes → should normalize before collision check
        var request = CreateMultiScopeRequest(
            scopes:
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "/infra/bicep/",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "content-a",
                    },
                },
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra/bicep",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "content-b",
                    },
                },
            ]);

        // Act
        var result = await _sut.PushScopedFilesAsync(request);

        // Assert — collision detected on normalized path
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Contain("collision");
    }

    [Fact]
    public async Task Given_EmptyBasePath_When_PushScopedFilesAsync_Then_UsesRelativePathDirectlyAsync()
    {
        // Arrange
        var request = CreateMultiScopeRequest(
            scopes:
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "content-a",
                    },
                },
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "content-b",
                    },
                },
            ]);

        // Act
        var result = await _sut.PushScopedFilesAsync(request);

        // Assert — collision on root-level path
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Contain("collision");
        result.FirstError.Description.Should().Contain("main.bicep");
    }

    // ──────────────────────────────────────────────────
    //  PushFilesAsync — delegation to PushScopedFilesAsync
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_EmptyFilesDictionary_When_PushFilesAsync_Then_ReturnsErrorAsync()
    {
        // Arrange
        var request = new GitPushRequest
        {
            Token = "fake-token",
            Owner = "test-owner",
            RepositoryName = "test-repo",
            BaseBranch = "main",
            TargetBranchName = "feature/gen",
            CommitMessage = "Generated",
            BasePath = "infra",
            Files = new Dictionary<string, string>(),
        };

        // Act
        var result = await _sut.PushFilesAsync(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Contain("No generated files were provided");
    }

    // ──────────────────────────────────────────────────
    //  TestConnectionAsync — error path
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_InvalidToken_When_TestConnectionAsync_Then_ReturnsFalseWithErrorMessageAsync()
    {
        // Arrange — Octokit will throw when called with an invalid token against real GitHub
        // We test that the catch block returns a failed TestGitConnectionResult

        // Act
        var result = await _sut.TestConnectionAsync("bad-token", "owner", "repo");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Success.Should().BeFalse();
        result.Value.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    // ──────────────────────────────────────────────────
    //  Multiple scopes — distinct paths, no collision
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_DistinctPathsAcrossScopes_When_PushScopedFilesAsync_Then_DoesNotReturnCollisionErrorAsync()
    {
        // Arrange
        var request = CreateMultiScopeRequest(
            scopes:
            [
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra/rg1",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource rg1 {}",
                    },
                },
                new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = "infra/rg2",
                    Files = new Dictionary<string, string>
                    {
                        ["main.bicep"] = "resource rg2 {}",
                    },
                },
            ]);

        // Act
        var result = await _sut.PushScopedFilesAsync(request);

        // Assert — passes validation but fails on API call (no real token)
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().NotContain("collision");
        result.FirstError.Description.Should().NotContain("No generated files");
    }

    private static MultiScopeGitPushRequest CreateMultiScopeRequest(
        IReadOnlyList<MultiScopeGitPushRequest.GitPushScope> scopes) =>
        new()
        {
            Token = "fake-token",
            Owner = "test-owner",
            RepositoryName = "test-repo",
            BaseBranch = "main",
            TargetBranchName = "feature/generated",
            CommitMessage = "Push generated files",
            Scopes = scopes,
        };
}
