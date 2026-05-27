using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.RemoveAppConfigurationKey;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.RemoveAppConfigurationKey;

public sealed class RemoveAppConfigurationKeyCommandValidatorTests
{
    private readonly RemoveAppConfigurationKeyCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new RemoveAppConfigurationKeyCommand(
            AzureResourceId.CreateUnique(),
            AppConfigurationKeyId.CreateUnique());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullAppConfigurationId_When_Validate_Then_Fails()
    {
        var command = new RemoveAppConfigurationKeyCommand(
            null!,
            AppConfigurationKeyId.CreateUnique());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveAppConfigurationKeyCommand.AppConfigurationId));
    }

    [Fact]
    public void Given_NullKeyId_When_Validate_Then_Fails()
    {
        var command = new RemoveAppConfigurationKeyCommand(
            AzureResourceId.CreateUnique(),
            null!);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveAppConfigurationKeyCommand.AppConfigurationKeyId));
    }
}
