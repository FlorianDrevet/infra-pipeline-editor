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
        var alias = RepositoryAlias.Create("infra").Value;
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = ProjectRepository.Create(
            ProjectId.CreateUnique(),
            alias,
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
        var alias = RepositoryAlias.Create("infra").Value;
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = ProjectRepository.Create(
            ProjectId.CreateUnique(),
            alias,
            new GitProviderType(GitProviderTypeEnum.AzureDevOps),
            "https://dev.azure.com/contoso/platform/_git/infra-repo",
            "main",
            contentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Owner.Should().Be("contoso/platform");
        result.Value.RepositoryName.Should().Be("infra-repo");
    }
}