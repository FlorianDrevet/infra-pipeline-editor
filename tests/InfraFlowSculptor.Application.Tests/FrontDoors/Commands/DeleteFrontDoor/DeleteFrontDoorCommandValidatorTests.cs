using FluentAssertions;
using InfraFlowSculptor.Application.FrontDoors.Commands.DeleteFrontDoor;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.FrontDoors.Commands.DeleteFrontDoor;

public sealed class DeleteFrontDoorCommandValidatorTests
{
    private readonly DeleteFrontDoorCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteFrontDoorCommand(
            AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteFrontDoorCommand(
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFrontDoorCommand.Id));
    }
}
