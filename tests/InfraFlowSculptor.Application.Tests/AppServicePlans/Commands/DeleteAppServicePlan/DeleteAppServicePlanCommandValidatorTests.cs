using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppServicePlans.Commands.DeleteAppServicePlan;

public sealed class DeleteAppServicePlanCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteAppServicePlanCommand.Id);

    private readonly DeleteAppServicePlanCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteAppServicePlanCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteAppServicePlanCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
