using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectRepository;

public sealed class AddProjectRepositoryCommandValidatorTests
{
    private const string ValidAlias = "infra-repo";
    private const string ValidProviderType = "GitHub";
    private const string ValidRepositoryUrl = "https://github.com/org/repo";
    private const string ValidDefaultBranch = "main";
    private const string ProjectIdProperty = nameof(AddProjectRepositoryCommand.ProjectId);
    private const string AliasProperty = nameof(AddProjectRepositoryCommand.Alias);
    private const string ContentKindsProperty = nameof(AddProjectRepositoryCommand.ContentKinds);

    private static readonly IReadOnlyList<string> ValidContentKinds = ["Infrastructure"];

    private readonly AddProjectRepositoryCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommandWithConnectionDetails_When_Validate_Then_Succeeds()
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
    public void Given_ValidCommandWithoutConnectionDetails_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand(providerType: null, repositoryUrl: null, defaultBranch: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = new AddProjectRepositoryCommand(
            null!, ValidAlias, ValidProviderType, ValidRepositoryUrl, ValidDefaultBranch, ValidContentKinds);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyAlias_When_Validate_Then_FailsOnAlias(string? alias)
    {
        // Arrange
        var command = CreateCommand(alias: alias!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == AliasProperty);
    }

    [Fact]
    public void Given_AliasLongerThan50Characters_When_Validate_Then_FailsOnAlias()
    {
        // Arrange
        var command = CreateCommand(alias: new string('a', 51));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == AliasProperty);
    }

    [Fact]
    public void Given_AliasWithUppercase_When_Validate_Then_FailsOnAlias()
    {
        // Arrange
        var command = CreateCommand(alias: "Infra-Repo");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == AliasProperty);
    }

    [Fact]
    public void Given_AliasWithSpecialChars_When_Validate_Then_FailsOnAlias()
    {
        // Arrange
        var command = CreateCommand(alias: "infra_repo!");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == AliasProperty);
    }

    [Fact]
    public void Given_InvalidProviderType_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(providerType: "GitLab");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_InvalidRepositoryUrl_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(repositoryUrl: "not-a-url");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_PartialConnectionDetails_When_Validate_Then_Fails()
    {
        // Arrange — URL provided but no provider/branch
        var command = CreateCommand(providerType: null, repositoryUrl: ValidRepositoryUrl, defaultBranch: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_NullContentKinds_When_Validate_Then_FailsOnContentKinds()
    {
        // Arrange
        var command = new AddProjectRepositoryCommand(
            ProjectId.CreateUnique(), ValidAlias, ValidProviderType, ValidRepositoryUrl, ValidDefaultBranch, null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ContentKindsProperty);
    }

    [Fact]
    public void Given_EmptyContentKindsList_When_Validate_Then_FailsOnContentKinds()
    {
        // Arrange
        var command = CreateCommand(contentKinds: []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ContentKindsProperty);
    }

    private static AddProjectRepositoryCommand CreateCommand(
        ProjectId? projectId = null,
        string alias = ValidAlias,
        string? providerType = ValidProviderType,
        string? repositoryUrl = ValidRepositoryUrl,
        string? defaultBranch = ValidDefaultBranch,
        IReadOnlyList<string>? contentKinds = null)
    {
        return new AddProjectRepositoryCommand(
            projectId ?? ProjectId.CreateUnique(),
            alias,
            providerType,
            repositoryUrl,
            defaultBranch,
            contentKinds ?? ValidContentKinds);
    }
}
