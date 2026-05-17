using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveCrossConfigReference;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.RemoveCrossConfigReference;

public sealed class RemoveCrossConfigReferenceCommandValidatorTests
{
    private readonly RemoveCrossConfigReferenceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new RemoveCrossConfigReferenceCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfraConfigId_When_Validate_Then_Fails()
    {
        var command = new RemoveCrossConfigReferenceCommand(Guid.Empty, Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveCrossConfigReferenceCommand.InfraConfigId));
    }

    [Fact]
    public void Given_EmptyReferenceId_When_Validate_Then_Fails()
    {
        var command = new RemoveCrossConfigReferenceCommand(Guid.NewGuid(), Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveCrossConfigReferenceCommand.ReferenceId));
    }
}
