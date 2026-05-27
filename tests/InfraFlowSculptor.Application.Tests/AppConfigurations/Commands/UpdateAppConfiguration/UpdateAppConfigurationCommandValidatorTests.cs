using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.UpdateAppConfiguration;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.UpdateAppConfiguration;

public sealed class UpdateAppConfigurationCommandValidatorTests
{
    private const string IdProperty = nameof(UpdateAppConfigurationCommand.Id);
    private const string NameProperty = nameof(UpdateAppConfigurationCommand.Name);

    private readonly UpdateAppConfigurationCommandValidator _sut = new();

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
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new UpdateAppConfigurationCommand(
            null!,
            new Name("appcs-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            EnvironmentSettings: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == IdProperty);
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new UpdateAppConfigurationCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.FranceCentral),
            EnvironmentSettings: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == NameProperty);
    }

    private static UpdateAppConfigurationCommand CreateCommand(
        AzureResourceId? id = null,
        Name? name = null,
        Location? location = null)
    {
        return new UpdateAppConfigurationCommand(
            id ?? AzureResourceId.CreateUnique(),
            name ?? new Name("appcs-shared"),
            location ?? new Location(Location.LocationEnum.FranceCentral),
            EnvironmentSettings: null);
    }
}
