using FluentAssertions;
using InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.RemoveRoleAssignment;

public sealed class RemoveRoleAssignmentCommandValidatorTests
{
    private readonly RemoveRoleAssignmentCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            RoleAssignmentId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptySourceResourceId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new RemoveRoleAssignmentCommand(
            null!,
            RoleAssignmentId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveRoleAssignmentCommand.SourceResourceId));
    }

    [Fact]
    public void Given_EmptyRoleAssignmentId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new RemoveRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveRoleAssignmentCommand.RoleAssignmentId));
    }
}
