using FluentAssertions;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.UpdateContainerRegistry;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerRegistries.Commands.UpdateContainerRegistry;

public sealed class UpdateContainerRegistryCommandValidatorTests
{
    private readonly UpdateContainerRegistryCommandValidator _sut = new();

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
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = CreateCommand() with { Id = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerRegistryCommand.Id));
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerRegistryCommand.Name));
    }

    private static UpdateContainerRegistryCommand CreateCommand()
    {
        return new UpdateContainerRegistryCommand(
            AzureResourceId.CreateUnique(),
            new Name("myregistry"),
            new Location(Location.LocationEnum.WestEurope));
    }
}
