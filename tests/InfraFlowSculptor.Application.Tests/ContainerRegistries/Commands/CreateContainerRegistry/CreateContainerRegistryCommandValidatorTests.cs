using FluentAssertions;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerRegistries.Commands.CreateContainerRegistry;

public sealed class CreateContainerRegistryCommandValidatorTests
{
    private readonly CreateContainerRegistryCommandValidator _sut = new();

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
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = CreateCommand() with { Name = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContainerRegistryCommand.Name));
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        // Arrange
        var command = CreateCommand() with { ResourceGroupId = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContainerRegistryCommand.ResourceGroupId));
    }

    private static CreateContainerRegistryCommand CreateCommand()
    {
        return new CreateContainerRegistryCommand(
            ResourceGroupId.CreateUnique(),
            new Name("myregistry"),
            new Location(Location.LocationEnum.WestEurope));
    }
}
