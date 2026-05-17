using FluentAssertions;
using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.CosmosDbs.Commands.CreateCosmosDb;

public sealed class CreateCosmosDbCommandValidatorTests
{
    private readonly CreateCosmosDbCommandValidator _sut = new();

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCosmosDbCommand.Name));
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCosmosDbCommand.ResourceGroupId));
    }

    private static CreateCosmosDbCommand CreateCommand()
    {
        return new CreateCosmosDbCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-cosmos-db"),
            new Location(Location.LocationEnum.WestEurope));
    }
}
