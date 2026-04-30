using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.Entities;

public sealed class InfraConfigRepositoryTests
{
    private const string ValidGitHubUrl = "https://github.com/acme/infra";
    private const string ValidGitHubUrlWithTrailingSlash = "https://github.com/acme/infra/";
    private const string DefaultBranch = "main";

    private static InfraConfigRepository CreateValidSut()
    {
        var configId = InfrastructureConfigId.CreateUnique();
        var alias = RepositoryAlias.Create("infra-repo").Value;
        var provider = new GitProviderType(GitProviderTypeEnum.GitHub);
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        return InfraConfigRepository.Create(configId, alias, provider, ValidGitHubUrl, DefaultBranch, contentKinds).Value;
    }

    [Fact]
    public void Given_ValidArguments_When_Create_Then_ReturnsRepositoryWithParsedOwnerAndName()
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var alias = RepositoryAlias.Create("infra-repo").Value;
        var provider = new GitProviderType(GitProviderTypeEnum.GitHub);
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = InfraConfigRepository.Create(configId, alias, provider, ValidGitHubUrl, DefaultBranch, contentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.InfrastructureConfigId.Should().Be(configId);
        result.Value.Alias.Should().Be(alias);
        result.Value.ProviderType.Value.Should().Be(GitProviderTypeEnum.GitHub);
        result.Value.RepositoryUrl.Should().Be(ValidGitHubUrl);
        result.Value.DefaultBranch.Should().Be(DefaultBranch);
        result.Value.Owner.Should().Be("acme");
        result.Value.RepositoryName.Should().Be("infra");
        result.Value.ContentKinds.Should().Be(contentKinds);
    }

    [Fact]
    public void Given_TrailingSlashInUrl_When_Create_Then_TrimsTrailingSlash()
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var alias = RepositoryAlias.Create("infra-repo").Value;
        var provider = new GitProviderType(GitProviderTypeEnum.GitHub);
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = InfraConfigRepository.Create(configId, alias, provider, ValidGitHubUrlWithTrailingSlash, DefaultBranch, contentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RepositoryUrl.Should().NotEndWith("/");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_BlankUrl_When_Create_Then_ReturnsValidationError(string url)
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var alias = RepositoryAlias.Create("infra-repo").Value;
        var provider = new GitProviderType(GitProviderTypeEnum.GitHub);
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = InfraConfigRepository.Create(configId, alias, provider, url, DefaultBranch, contentKinds);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_BlankDefaultBranch_When_Create_Then_ReturnsValidationError(string branch)
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var alias = RepositoryAlias.Create("infra-repo").Value;
        var provider = new GitProviderType(GitProviderTypeEnum.GitHub);
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = InfraConfigRepository.Create(configId, alias, provider, ValidGitHubUrl, branch, contentKinds);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingRepository_When_Update_Then_ReplacesMutableFields()
    {
        // Arrange
        var sut = CreateValidSut();
        const string newUrl = "https://github.com/contoso/code";
        const string newBranch = "develop";
        var newProvider = new GitProviderType(GitProviderTypeEnum.GitHub);
        var newContentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.ApplicationCode).Value;

        // Act
        var result = sut.Update(newProvider, newUrl, newBranch, newContentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        sut.RepositoryUrl.Should().Be(newUrl);
        sut.DefaultBranch.Should().Be(newBranch);
        sut.Owner.Should().Be("contoso");
        sut.RepositoryName.Should().Be("code");
        sut.ContentKinds.Should().Be(newContentKinds);
    }

    [Fact]
    public void Given_ExistingRepository_When_UpdateWithBlankUrl_Then_ReturnsValidationError()
    {
        // Arrange
        var sut = CreateValidSut();
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = sut.Update(new GitProviderType(GitProviderTypeEnum.GitHub), "", DefaultBranch, contentKinds);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingRepository_When_UpdateWithBlankBranch_Then_ReturnsValidationError()
    {
        // Arrange
        var sut = CreateValidSut();
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = sut.Update(new GitProviderType(GitProviderTypeEnum.GitHub), ValidGitHubUrl, "", contentKinds);

        // Assert
        result.IsError.Should().BeTrue();
    }
}
