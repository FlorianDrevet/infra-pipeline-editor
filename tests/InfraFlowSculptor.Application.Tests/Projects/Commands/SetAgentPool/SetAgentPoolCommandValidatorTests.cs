using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.SetAgentPool;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetAgentPool;

public sealed class SetAgentPoolCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string AgentPoolNameProperty = nameof(SetAgentPoolCommand.AgentPoolName);

    private readonly SetAgentPoolCommandValidator _sut = new();

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
    public void Given_NullAgentPoolName_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand(agentPoolName: null);

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
    public void Given_AgentPoolNameLongerThan200Characters_When_Validate_Then_FailsOnAgentPoolName()
    {
        // Arrange
        var command = CreateCommand(agentPoolName: new string('a', 201));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == AgentPoolNameProperty);
    }

    private static SetAgentPoolCommand CreateCommand(
        ProjectId? projectId = null,
        string? agentPoolName = "self-hosted-pool")
    {
        return new SetAgentPoolCommand(
            projectId ?? new ProjectId(Guid.NewGuid()),
            agentPoolName);
    }
}
