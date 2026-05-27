using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.UpdateProjectMemberRole;

public sealed class UpdateProjectMemberRoleCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(UpdateProjectMemberRoleCommand.ProjectId);
    private const string UserIdProperty = nameof(UpdateProjectMemberRoleCommand.UserId);
    private const string NewRoleProperty = nameof(UpdateProjectMemberRoleCommand.NewRole);

    private readonly UpdateProjectMemberRoleCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateProjectMemberRoleCommand(
            ProjectId.CreateUnique(),
            Guid.NewGuid(),
            "Admin");

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
        var command = new UpdateProjectMemberRoleCommand(
            null!,
            Guid.NewGuid(),
            "Admin");

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
        var command = new UpdateProjectMemberRoleCommand(
            ProjectId.CreateUnique(),
            Guid.Empty,
            "Admin");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == UserIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyNewRole_When_Validate_Then_FailsOnNewRole(string? newRole)
    {
        // Arrange
        var command = new UpdateProjectMemberRoleCommand(
            ProjectId.CreateUnique(),
            Guid.NewGuid(),
            newRole!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == NewRoleProperty);
    }
}
