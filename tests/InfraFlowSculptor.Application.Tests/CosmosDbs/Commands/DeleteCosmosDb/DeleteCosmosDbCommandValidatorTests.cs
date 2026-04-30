using FluentAssertions;
using InfraFlowSculptor.Application.CosmosDbs.Commands.DeleteCosmosDb;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.CosmosDbs.Commands.DeleteCosmosDb;

public sealed class DeleteCosmosDbCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteCosmosDbCommand.Id);

    private readonly DeleteCosmosDbCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteCosmosDbCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteCosmosDbCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
