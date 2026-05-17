using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.UpdateProjectRepository;

public sealed class UpdateProjectRepositoryCommandValidatorTests
{
    private const string ValidProviderType = "GitHub";
    private const string ValidRepositoryUrl = "https://github.com/org/repo";
    private const string ValidDefaultBranch = "main";
    private const string ProjectIdProperty = nameof(UpdateProjectRepositoryCommand.ProjectId);
    private const string RepositoryIdProperty = nameof(UpdateProjectRepositoryCommand.RepositoryId);
    private const string ContentKindsProperty = nameof(UpdateProjectRepositoryCommand.ContentKinds);

    private static readonly IReadOnlyList<string> ValidContentKinds = ["Infrastructure"];

    private readonly UpdateProjectRepositoryCommandValidator _sut = new();

    private static UpdateProjectRepositoryCommand CreateCommand(
        ProjectId? projectId = null,
        ProjectRepositoryId? repositoryId = null,
        string? providerType = ValidProviderType,
        string? repositoryUrl = ValidRepositoryUrl,
        string? defaultBranch = ValidDefaultBranch,
        IReadOnlyList<string>? contentKinds = null)
    {
        return new UpdateProjectRepositoryCommand(
            projectId ?? ProjectId.CreateUnique(),
            repositoryId ?? ProjectRepositoryId.CreateUnique(),
            providerType,
            repositoryUrl,
            defaultBranch,
            contentKinds ?? ValidContentKinds);
    }

    [Fact]
    public void Given_ValidCommandWithConnection_When_Validate_Then_Succeeds()
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
    public void Given_ValidCommandWithoutConnection_When_Validate_Then_Succeeds()
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
        var command = new UpdateProjectRepositoryCommand(
            null!,
            ProjectRepositoryId.CreateUnique(),
            ValidProviderType,
            ValidRepositoryUrl,
            ValidDefaultBranch,
            ValidContentKinds);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_NullRepositoryId_When_Validate_Then_FailsOnRepositoryId()
    {
        // Arrange
        var command = new UpdateProjectRepositoryCommand(
            ProjectId.CreateUnique(),
            null!,
            ValidProviderType,
            ValidRepositoryUrl,
            ValidDefaultBranch,
            ValidContentKinds);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RepositoryIdProperty);
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
        var command = new UpdateProjectRepositoryCommand(
            ProjectId.CreateUnique(),
            ProjectRepositoryId.CreateUnique(),
            ValidProviderType,
            ValidRepositoryUrl,
            ValidDefaultBranch,
            null!);

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
}
