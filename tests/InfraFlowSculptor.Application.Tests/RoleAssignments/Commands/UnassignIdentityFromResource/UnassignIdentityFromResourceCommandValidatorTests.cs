using FluentAssertions;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UnassignIdentityFromResource;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.UnassignIdentityFromResource;

public sealed class UnassignIdentityFromResourceCommandValidatorTests
{
    private readonly UnassignIdentityFromResourceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UnassignIdentityFromResourceCommand(
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
        var command = new UnassignIdentityFromResourceCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UnassignIdentityFromResourceCommand.ResourceId));
    }
}
