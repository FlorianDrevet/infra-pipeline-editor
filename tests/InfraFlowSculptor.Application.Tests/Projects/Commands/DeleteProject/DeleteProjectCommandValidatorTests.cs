using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.DeleteProject;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(DeleteProjectCommand.ProjectId);

    private readonly DeleteProjectCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullProjectId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteProjectCommand(ProjectId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = new DeleteProjectCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }
}
