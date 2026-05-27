using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectGitPat;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectGitPat;

public sealed class SetProjectGitPatCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(SetProjectGitPatCommand.ProjectId);
    private const string RepositoryIdProperty = nameof(SetProjectGitPatCommand.RepositoryId);
    private const string PersonalAccessTokenProperty = nameof(SetProjectGitPatCommand.PersonalAccessToken);

    private readonly SetProjectGitPatCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new SetProjectGitPatCommand(ProjectId.CreateUnique(), ProjectRepositoryId.CreateUnique(), "ghp_token123");

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
        var command = new SetProjectGitPatCommand(null!, ProjectRepositoryId.CreateUnique(), "ghp_token123");

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
        var command = new SetProjectGitPatCommand(ProjectId.CreateUnique(), null!, "ghp_token123");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RepositoryIdProperty);
    }

    [Fact]
    public void Given_EmptyPersonalAccessToken_When_Validate_Then_FailsOnPersonalAccessToken()
    {
        // Arrange
        var command = new SetProjectGitPatCommand(ProjectId.CreateUnique(), ProjectRepositoryId.CreateUnique(), string.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == PersonalAccessTokenProperty);
    }
}