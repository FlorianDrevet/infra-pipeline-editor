using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.AddInfraConfigRepository;

public sealed class AddInfraConfigRepositoryCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string ConfigIdProperty = "ConfigId.Value";
    private const string ProviderTypeProperty = nameof(AddInfraConfigRepositoryCommand.ProviderType);
    private const string RepositoryUrlProperty = nameof(AddInfraConfigRepositoryCommand.RepositoryUrl);
    private const string ContentKindsProperty = nameof(AddInfraConfigRepositoryCommand.ContentKinds);

    private readonly AddInfraConfigRepositoryCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = CreateCommand(projectId: new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_EmptyConfigId_When_Validate_Then_FailsOnConfigId()
    {
        // Arrange
        var command = CreateCommand(configId: new InfrastructureConfigId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == ConfigIdProperty);
    }

    public void Given_InvalidProviderType_When_Validate_Then_FailsOnProviderType()
    {
        // Arrange
        var command = CreateCommand(providerType: "Bitbucket");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == ProviderTypeProperty || error.PropertyName == string.Empty);
    }

    [Fact]
    public void Given_RelativeRepositoryUrl_When_Validate_Then_FailsOnRepositoryUrl()
    {
        // Arrange
        var command = CreateCommand(repositoryUrl: "/infra/repo");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == RepositoryUrlProperty);
    }

    [Fact]
    public void Given_EmptyContentKinds_When_Validate_Then_FailsOnContentKinds()
    {
        // Arrange
        var command = CreateCommand(contentKinds: []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == ContentKindsProperty);
    }

    private static AddInfraConfigRepositoryCommand CreateCommand(
        ProjectId? projectId = null,
        InfrastructureConfigId? configId = null,
        string providerType = nameof(GitProviderTypeEnum.GitHub),
        string repositoryUrl = "https://github.com/octo-org/retail-platform-infra",
        string defaultBranch = "main",
        IReadOnlyList<string>? contentKinds = null)
    {
        return new AddInfraConfigRepositoryCommand(
            projectId ?? new ProjectId(Guid.NewGuid()),
            configId ?? new InfrastructureConfigId(Guid.NewGuid()),
            providerType,
            repositoryUrl,
            defaultBranch,
            contentKinds ?? [nameof(RepositoryContentKindsEnum.Infrastructure)]);
    }
}
