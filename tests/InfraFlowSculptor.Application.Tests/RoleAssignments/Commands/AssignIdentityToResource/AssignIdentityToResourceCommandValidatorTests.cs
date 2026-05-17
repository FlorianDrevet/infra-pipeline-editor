using FluentAssertions;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AssignIdentityToResource;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.AssignIdentityToResource;

public sealed class AssignIdentityToResourceCommandValidatorTests
{
    private readonly AssignIdentityToResourceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AssignIdentityToResourceCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullResourceId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AssignIdentityToResourceCommand(
            null!,
            AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignIdentityToResourceCommand.ResourceId));
    }

    [Fact]
    public void Given_NullUserAssignedIdentityId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AssignIdentityToResourceCommand(
            AzureResourceId.CreateUnique(),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignIdentityToResourceCommand.UserAssignedIdentityId));
    }
}
