using FluentAssertions;
using InfraFlowSculptor.Application.CosmosDbs.Commands.UpdateCosmosDb;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.CosmosDbs.Commands.UpdateCosmosDb;

public sealed class UpdateCosmosDbCommandValidatorTests
{
    private readonly UpdateCosmosDbCommandValidator _sut = new();

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCosmosDbCommand.Id));
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCosmosDbCommand.Name));
    }

    private static UpdateCosmosDbCommand CreateCommand()
    {
        return new UpdateCosmosDbCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-cosmos-db"),
            new Location(Location.LocationEnum.WestEurope));
    }
}
