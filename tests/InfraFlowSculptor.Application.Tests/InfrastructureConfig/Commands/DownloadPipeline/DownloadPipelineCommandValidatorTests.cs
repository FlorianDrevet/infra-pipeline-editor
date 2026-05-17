using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadPipeline;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.DownloadPipeline;

public sealed class DownloadPipelineCommandValidatorTests
{
    private readonly DownloadPipelineCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new DownloadPipelineCommand(Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_FailsOnInfrastructureConfigId()
    {
        var command = new DownloadPipelineCommand(Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DownloadPipelineCommand.InfrastructureConfigId));
    }
}
