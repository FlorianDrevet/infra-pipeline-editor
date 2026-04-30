using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.DeleteAppConfiguration;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.DeleteAppConfiguration;

public sealed class DeleteAppConfigurationCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteAppConfigurationCommand.Id);

    private readonly DeleteAppConfigurationCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteAppConfigurationCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteAppConfigurationCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
