using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBootstrap;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.DownloadBootstrap;

public sealed class DownloadBootstrapCommandValidatorTests
{
    private readonly DownloadBootstrapCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new DownloadBootstrapCommand(Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_FailsOnInfrastructureConfigId()
    {
        var command = new DownloadBootstrapCommand(Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DownloadBootstrapCommand.InfrastructureConfigId));
    }
}
