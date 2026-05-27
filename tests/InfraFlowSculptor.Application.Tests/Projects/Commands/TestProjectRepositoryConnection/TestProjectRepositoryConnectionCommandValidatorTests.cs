using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.TestProjectRepositoryConnection;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.TestProjectRepositoryConnection;

public sealed class TestProjectRepositoryConnectionCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(TestProjectRepositoryConnectionCommand.ProjectId);
    private const string RepositoryIdProperty = nameof(TestProjectRepositoryConnectionCommand.RepositoryId);

    private readonly TestProjectRepositoryConnectionCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new TestProjectRepositoryConnectionCommand(ProjectId.CreateUnique(), ProjectRepositoryId.CreateUnique());

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
        var command = new TestProjectRepositoryConnectionCommand(null!, ProjectRepositoryId.CreateUnique());

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
        var command = new TestProjectRepositoryConnectionCommand(ProjectId.CreateUnique(), null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RepositoryIdProperty);
    }
}