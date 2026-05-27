using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.Entities;

public sealed class ProjectRepositoryTests
{
    [Fact]
    public void Given_GitHubRepositoryUrl_When_Create_Then_ExtractsOwnerAndRepositoryName()
    {
        // Arrange
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = ProjectRepository.Create(
            ProjectId.CreateUnique(),
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/octo/infra-repo.git",
            "main",
            contentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Owner.Should().Be("octo");
        result.Value.RepositoryName.Should().Be("infra-repo");
    }

    [Fact]
    public void Given_AzureDevOpsRepositoryUrl_When_Create_Then_ExtractsCompositeOwnerAndRepositoryName()
    {
        // Arrange
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = ProjectRepository.Create(
            ProjectId.CreateUnique(),
            new GitProviderType(GitProviderTypeEnum.AzureDevOps),
            "https://dev.azure.com/contoso/platform/_git/infra-repo",
            "main",
            contentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Owner.Should().Be("contoso/platform");
        result.Value.RepositoryName.Should().Be("infra-repo");
    }

    [Fact]
    public void Given_UnconfiguredSlot_When_Create_Then_ReturnsRepositoryWithoutConnectionDetails()
    {
        // Arrange
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = ProjectRepository.Create(
            ProjectId.CreateUnique(),
            providerType: null,
            repositoryUrl: null,
            defaultBranch: null,
            contentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsConfigured.Should().BeFalse();
        result.Value.ProviderType.Should().BeNull();
        result.Value.RepositoryUrl.Should().BeNull();
        result.Value.DefaultBranch.Should().BeNull();
    }
}
