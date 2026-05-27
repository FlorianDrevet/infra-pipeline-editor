using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetInheritance;

public sealed class SetInheritanceCommandValidatorTests
{
    private readonly SetInheritanceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new SetInheritanceCommand(InfrastructureConfigId.CreateUnique(), true);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfraConfigId_When_Validate_Then_Fails()
    {
        var command = new SetInheritanceCommand(InfrastructureConfigId.Create(Guid.Empty), true);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InfraConfigId.Value");
    }
}
