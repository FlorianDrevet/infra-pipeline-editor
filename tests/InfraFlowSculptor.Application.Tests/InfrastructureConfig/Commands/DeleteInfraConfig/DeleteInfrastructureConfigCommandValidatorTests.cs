using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DeleteInfraConfig;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.DeleteInfraConfig;

public sealed class DeleteInfrastructureConfigCommandValidatorTests
{
    private const string InfraConfigIdProperty = nameof(DeleteInfrastructureConfigCommand.InfraConfigId);

    private readonly DeleteInfrastructureConfigCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullInfraConfigId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteInfrastructureConfigCommand(InfrastructureConfigId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullInfraConfigId_When_Validate_Then_FailsOnInfraConfigId()
    {
        // Arrange
        var command = new DeleteInfrastructureConfigCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == InfraConfigIdProperty);
    }
}
