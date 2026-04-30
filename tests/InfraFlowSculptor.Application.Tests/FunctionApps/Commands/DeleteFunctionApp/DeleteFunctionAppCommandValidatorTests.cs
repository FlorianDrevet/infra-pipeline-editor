using FluentAssertions;
using InfraFlowSculptor.Application.FunctionApps.Commands.DeleteFunctionApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.FunctionApps.Commands.DeleteFunctionApp;

public sealed class DeleteFunctionAppCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteFunctionAppCommand.Id);

    private readonly DeleteFunctionAppCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteFunctionAppCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteFunctionAppCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
