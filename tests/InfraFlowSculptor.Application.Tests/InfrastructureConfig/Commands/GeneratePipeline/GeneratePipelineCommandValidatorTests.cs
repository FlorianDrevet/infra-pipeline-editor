using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.GeneratePipeline;

public sealed class GeneratePipelineCommandValidatorTests
{
    private readonly GeneratePipelineCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new GeneratePipelineCommand(Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_FailsOnInfrastructureConfigId()
    {
        var command = new GeneratePipelineCommand(Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GeneratePipelineCommand.InfrastructureConfigId));
    }
}
