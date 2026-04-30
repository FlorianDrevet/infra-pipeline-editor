using FluentAssertions;
using InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ResourceGroups.Commands.DeleteResourceGroup;

public sealed class DeleteResourceGroupCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteResourceGroupCommand.Id);

    private readonly DeleteResourceGroupCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteResourceGroupCommand(ResourceGroupId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteResourceGroupCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
