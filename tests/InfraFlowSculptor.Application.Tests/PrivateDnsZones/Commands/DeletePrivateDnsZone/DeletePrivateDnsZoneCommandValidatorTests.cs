using FluentAssertions;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.DeletePrivateDnsZone;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.PrivateDnsZones.Commands.DeletePrivateDnsZone;

public sealed class DeletePrivateDnsZoneCommandValidatorTests
{
    private readonly DeletePrivateDnsZoneCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeletePrivateDnsZoneCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeletePrivateDnsZoneCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeletePrivateDnsZoneCommand.Id));
    }
}
