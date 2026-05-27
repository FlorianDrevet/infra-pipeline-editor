using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableGroup;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.RemoveProjectPipelineVariableGroup;

public sealed class RemoveProjectPipelineVariableGroupCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(RemoveProjectPipelineVariableGroupCommand.ProjectId);
    private const string GroupIdProperty = nameof(RemoveProjectPipelineVariableGroupCommand.GroupId);

    private readonly RemoveProjectPipelineVariableGroupCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveProjectPipelineVariableGroupCommand(ProjectId.CreateUnique(), Guid.NewGuid());

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
        var command = new RemoveProjectPipelineVariableGroupCommand(null!, Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_EmptyGroupId_When_Validate_Then_FailsOnGroupId()
    {
        // Arrange
        var command = new RemoveProjectPipelineVariableGroupCommand(ProjectId.CreateUnique(), Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == GroupIdProperty);
    }
}
