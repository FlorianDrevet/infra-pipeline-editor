using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Commands.UpdateStaticAppSetting;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppSettings.Commands.UpdateStaticAppSetting;

public sealed class UpdateStaticAppSettingCommandValidatorTests
{
    private readonly UpdateStaticAppSettingCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new UpdateStaticAppSettingCommand(
            AzureResourceId.CreateUnique(),
            AppSettingId.CreateUnique(),
            "MY_SETTING",
            new Dictionary<string, string> { ["dev"] = "value1" });

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullResourceId_When_Validate_Then_Fails()
    {
        var command = new UpdateStaticAppSettingCommand(
            null!,
            AppSettingId.CreateUnique(),
            "MY_SETTING",
            new Dictionary<string, string> { ["dev"] = "value1" });

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStaticAppSettingCommand.ResourceId));
    }

    [Fact]
    public void Given_NullAppSettingId_When_Validate_Then_Fails()
    {
        var command = new UpdateStaticAppSettingCommand(
            AzureResourceId.CreateUnique(),
            null!,
            "MY_SETTING",
            new Dictionary<string, string> { ["dev"] = "value1" });

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStaticAppSettingCommand.AppSettingId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_Fails()
    {
        var command = new UpdateStaticAppSettingCommand(
            AzureResourceId.CreateUnique(),
            AppSettingId.CreateUnique(),
            "",
            new Dictionary<string, string> { ["dev"] = "value1" });

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStaticAppSettingCommand.Name));
    }
}
