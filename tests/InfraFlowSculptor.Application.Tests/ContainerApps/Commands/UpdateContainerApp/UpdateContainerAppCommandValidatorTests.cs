using FluentAssertions;
using InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerApps.Commands.UpdateContainerApp;

public sealed class UpdateContainerAppCommandValidatorTests
{
    private readonly UpdateContainerAppCommandValidator _sut = new();

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerAppCommand.Id));
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerAppCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        // Arrange
        var command = CreateCommand() with { Location = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerAppCommand.Location));
    }

    [Fact]
    public void Given_EmptyContainerAppEnvironmentId_When_Validate_Then_FailsOnContainerAppEnvironmentId()
    {
        // Arrange
        var command = CreateCommand() with { ContainerAppEnvironmentId = Guid.Empty };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerAppCommand.ContainerAppEnvironmentId));
    }

    [Fact]
    public void Given_EmptyContainerRegistryIdWhenProvided_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(containerRegistryId: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ContainerRegistryId"));
    }

    private static UpdateContainerAppCommand CreateCommand(
        Guid? containerAppEnvironmentId = null,
        Guid? containerRegistryId = null)
    {
        return new UpdateContainerAppCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-container-app"),
            new Location(Location.LocationEnum.WestEurope),
            containerAppEnvironmentId ?? Guid.NewGuid(),
            containerRegistryId);
    }
}
