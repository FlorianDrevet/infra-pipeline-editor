using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.AddCrossConfigReference;

public sealed class AddCrossConfigReferenceCommandValidatorTests
{
    private readonly AddCrossConfigReferenceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new AddCrossConfigReferenceCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfraConfigId_When_Validate_Then_FailsOnInfraConfigId()
    {
        var command = new AddCrossConfigReferenceCommand(Guid.Empty, Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCrossConfigReferenceCommand.InfraConfigId));
    }

    [Fact]
    public void Given_EmptyTargetResourceId_When_Validate_Then_FailsOnTargetResourceId()
    {
        var command = new AddCrossConfigReferenceCommand(Guid.NewGuid(), Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCrossConfigReferenceCommand.TargetResourceId));
    }
}
