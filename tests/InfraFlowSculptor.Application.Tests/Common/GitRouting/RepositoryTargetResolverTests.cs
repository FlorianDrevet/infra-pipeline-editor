using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Common.GitRouting;

public sealed class RepositoryTargetResolverTests
{
    private readonly RepositoryTargetResolver _sut = new();

    [Fact]
    public void Given_MultiRepoProjectWithConfigRepository_When_Resolve_Then_ReturnsInfraConfigRepositoryPatSecretName()
    {
        // Arrange
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        var config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);
        config.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;
        var configRepository = config.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/floriandrevet/infra",
            "main",
            contentKinds).Value;

        // Act
        var result = _sut.Resolve(project, config, ArtifactKind.Infrastructure);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.PatSecretName.Should().Be(
            ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(configRepository.Id));
        result.Value.PatSecretName.Should().NotBeNull();
    }

    [Fact]
    public void Given_MultiRepoProjectWithNoConfigRepository_When_Resolve_Then_ReturnsNoRepositoryConfiguredError()
    {
        // Arrange
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        var config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);

        // Act
        var result = _sut.Resolve(project, config, ArtifactKind.Infrastructure);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.NoRepositoryConfigured(project.Id).Code);
    }

    [Fact]
    public void Given_AllInOneProjectWithConfiguredRepository_When_Resolve_Then_ReturnsProjectRepositoryPatSecretName()
    {
        // Arrange
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));
        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;
        var projectRepository = project.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/floriandrevet/infra",
            "main",
            contentKinds).Value;

        // Act
        var result = _sut.Resolve(project, config: null, ArtifactKind.Infrastructure);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.PatSecretName.Should().Be(
            ProjectGitSecretNames.GetRepositoryPatSecretName(projectRepository.Id));
    }
}
