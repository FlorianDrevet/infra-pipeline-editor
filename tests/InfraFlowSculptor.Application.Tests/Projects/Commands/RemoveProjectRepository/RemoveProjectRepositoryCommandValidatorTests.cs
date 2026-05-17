using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectRepository;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.RemoveProjectRepository;

public sealed class RemoveProjectRepositoryCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(RemoveProjectRepositoryCommand.ProjectId);
    private const string RepositoryIdProperty = nameof(RemoveProjectRepositoryCommand.RepositoryId);

    private readonly RemoveProjectRepositoryCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveProjectRepositoryCommand(
            ProjectId.CreateUnique(),
            ProjectRepositoryId.CreateUnique());

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
        var command = new RemoveProjectRepositoryCommand(
            null!,
            ProjectRepositoryId.CreateUnique());

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
        var command = new RemoveProjectRepositoryCommand(
            ProjectId.CreateUnique(),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RepositoryIdProperty);
    }
}
