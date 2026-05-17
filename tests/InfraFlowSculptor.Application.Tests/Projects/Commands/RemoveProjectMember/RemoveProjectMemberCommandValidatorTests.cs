using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.RemoveProjectMember;

public sealed class RemoveProjectMemberCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string UserIdProperty = nameof(RemoveProjectMemberCommand.UserId);

    private readonly RemoveProjectMemberCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveProjectMemberCommand(ProjectId.CreateUnique(), Guid.NewGuid());

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
        var command = new RemoveProjectMemberCommand(new ProjectId(Guid.Empty), Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_EmptyUserId_When_Validate_Then_FailsOnUserId()
    {
        // Arrange
        var command = new RemoveProjectMemberCommand(ProjectId.CreateUnique(), Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == UserIdProperty);
    }
}
