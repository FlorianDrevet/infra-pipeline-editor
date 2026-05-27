using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Commands.RemoveAppSetting;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppSettings.Commands.RemoveAppSetting;

public sealed class RemoveAppSettingCommandValidatorTests
{
    private readonly RemoveAppSettingCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new RemoveAppSettingCommand(
            AzureResourceId.CreateUnique(),
            AppSettingId.CreateUnique());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullResourceId_When_Validate_Then_Fails()
    {
        var command = new RemoveAppSettingCommand(
            null!,
            AppSettingId.CreateUnique());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveAppSettingCommand.ResourceId));
    }

    [Fact]
    public void Given_NullAppSettingId_When_Validate_Then_Fails()
    {
        var command = new RemoveAppSettingCommand(
            AzureResourceId.CreateUnique(),
            null!);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveAppSettingCommand.AppSettingId));
    }
}
