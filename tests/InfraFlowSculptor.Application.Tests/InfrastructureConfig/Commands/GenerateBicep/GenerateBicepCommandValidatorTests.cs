using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.GenerateBicep;

public sealed class GenerateBicepCommandValidatorTests
{
    private readonly GenerateBicepCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new GenerateBicepCommand(Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_FailsOnInfrastructureConfigId()
    {
        var command = new GenerateBicepCommand(Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GenerateBicepCommand.InfrastructureConfigId));
    }
}
