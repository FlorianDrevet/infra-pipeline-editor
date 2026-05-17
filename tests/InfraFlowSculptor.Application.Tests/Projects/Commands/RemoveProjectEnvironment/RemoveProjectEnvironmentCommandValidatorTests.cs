using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.RemoveProjectEnvironment;

public sealed class RemoveProjectEnvironmentCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string EnvironmentIdProperty = "EnvironmentId.Value";

    private readonly RemoveProjectEnvironmentCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveProjectEnvironmentCommand(
            ProjectId.CreateUnique(),
            ProjectEnvironmentDefinitionId.CreateUnique());

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
        var command = new RemoveProjectEnvironmentCommand(
            new ProjectId(Guid.Empty),
            ProjectEnvironmentDefinitionId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_EmptyEnvironmentId_When_Validate_Then_FailsOnEnvironmentId()
    {
        // Arrange
        var command = new RemoveProjectEnvironmentCommand(
            ProjectId.CreateUnique(),
            new ProjectEnvironmentDefinitionId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == EnvironmentIdProperty);
    }
}
