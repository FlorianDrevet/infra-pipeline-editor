using FluentAssertions;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;

public sealed class CreateServiceBusNamespaceCommandValidatorTests
{
    private readonly CreateServiceBusNamespaceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateServiceBusNamespaceCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-servicebus"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new CreateServiceBusNamespaceCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateServiceBusNamespaceCommand.Name));
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new CreateServiceBusNamespaceCommand(
            null!,
            new Name("my-servicebus"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateServiceBusNamespaceCommand.ResourceGroupId));
    }
}
