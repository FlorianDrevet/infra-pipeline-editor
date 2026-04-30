using FluentAssertions;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.DeleteApplicationInsights;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ApplicationInsights.Commands.DeleteApplicationInsights;

public sealed class DeleteApplicationInsightsCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteApplicationInsightsCommand.Id);

    private readonly DeleteApplicationInsightsCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteApplicationInsightsCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteApplicationInsightsCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
