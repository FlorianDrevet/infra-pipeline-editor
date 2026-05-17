using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableGroup;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectPipelineVariableGroup;

public sealed class AddProjectPipelineVariableGroupCommandValidatorTests
{
    private const string ValidGroupName = "shared-variables";
    private const string ProjectIdProperty = nameof(AddProjectPipelineVariableGroupCommand.ProjectId);
    private const string GroupNameProperty = nameof(AddProjectPipelineVariableGroupCommand.GroupName);

    private readonly AddProjectPipelineVariableGroupCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddProjectPipelineVariableGroupCommand(ProjectId.CreateUnique(), ValidGroupName);

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
        var command = new AddProjectPipelineVariableGroupCommand(null!, ValidGroupName);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyGroupName_When_Validate_Then_FailsOnGroupName(string? groupName)
    {
        // Arrange
        var command = new AddProjectPipelineVariableGroupCommand(ProjectId.CreateUnique(), groupName!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == GroupNameProperty);
    }

    [Fact]
    public void Given_GroupNameLongerThan200Characters_When_Validate_Then_FailsOnGroupName()
    {
        // Arrange
        var command = new AddProjectPipelineVariableGroupCommand(ProjectId.CreateUnique(), new string('A', 201));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == GroupNameProperty);
    }
}
