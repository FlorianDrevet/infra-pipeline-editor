using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Helpers;

namespace InfraFlowSculptor.Application.Tests.Common.Helpers;

public sealed class MultiScopeGitPushRequestBuilderTests
{
    [Fact]
    public void Given_RootAndNestedScopes_When_Build_Then_SplitsRootFilesByTopLevelFolder()
    {
        // Arrange
        IReadOnlyList<(string? BasePath, IReadOnlyDictionary<string, string> Files)> scopes =
        [
            (
                BasePath: null,
                Files: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [".azuredevops/main.yml"] = "pipeline-main",
                    ["apps/api/ci.yml"] = "app-ci",
                }
            ),
            (
                BasePath: "infra-root",
                Files: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["main.bicep"] = "bicep-main",
                }
            ),
        ];

        // Act
        var result = MultiScopeGitPushRequestBuilder.Build(
            token: "token",
            owner: "owner",
            repositoryName: "repository",
            baseBranch: "main",
            targetBranchName: "feature/app-005",
            commitMessage: "commit",
            scopes);

        // Assert
        result.IsError.Should().BeFalse();
        var scopesByBasePath = result.Value.Scopes.ToDictionary(
            scope => scope.BasePath ?? string.Empty,
            scope => scope.Files,
            StringComparer.Ordinal);

        scopesByBasePath.Should().ContainKey(".azuredevops");
        scopesByBasePath[".azuredevops"].Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new KeyValuePair<string, string>("main.yml", "pipeline-main"));
        scopesByBasePath.Should().ContainKey("apps");
        scopesByBasePath["apps"].Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new KeyValuePair<string, string>("api/ci.yml", "app-ci"));
        scopesByBasePath.Should().ContainKey("infra-root");
        scopesByBasePath["infra-root"].Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new KeyValuePair<string, string>("main.bicep", "bicep-main"));
    }

    [Fact]
    public void Given_SameResolvedPathWithDifferentContent_When_Build_Then_ReturnsCollisionFailure()
    {
        // Arrange
        IReadOnlyList<(string? BasePath, IReadOnlyDictionary<string, string> Files)> scopes =
        [
            (
                BasePath: null,
                Files: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["infra/main.bicep"] = "first-content",
                }
            ),
            (
                BasePath: "infra",
                Files: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["main.bicep"] = "second-content",
                }
            ),
        ];

        // Act
        var result = MultiScopeGitPushRequestBuilder.Build(
            token: "token",
            owner: "owner",
            repositoryName: "repository",
            baseBranch: "main",
            targetBranchName: "feature/app-005",
            commitMessage: "commit",
            scopes);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Code.Should().Be("GitRepository.PushFailed");
        result.FirstError.Description.Should().Contain("Generated file collision detected for path 'infra/main.bicep'.");
    }
}