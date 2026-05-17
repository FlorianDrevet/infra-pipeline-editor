using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;

public sealed class SetResourceAbbreviationOverrideCommandValidatorTests
{
    private readonly SetResourceAbbreviationOverrideCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new SetResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", "kv");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceType_When_Validate_Then_Fails()
    {
        var command = new SetResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "", "kv");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceAbbreviationOverrideCommand.ResourceType));
    }

    [Fact]
    public void Given_ResourceTypeTooLong_When_Validate_Then_Fails()
    {
        var command = new SetResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), new string('x', 101), "kv");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceAbbreviationOverrideCommand.ResourceType));
    }

    [Fact]
    public void Given_EmptyAbbreviation_When_Validate_Then_Fails()
    {
        var command = new SetResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceAbbreviationOverrideCommand.Abbreviation));
    }

    [Fact]
    public void Given_AbbreviationTooLong_When_Validate_Then_Fails()
    {
        var command = new SetResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", new string('a', 11));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceAbbreviationOverrideCommand.Abbreviation));
    }

    [Fact]
    public void Given_AbbreviationWithUppercase_When_Validate_Then_Fails()
    {
        var command = new SetResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", "ABC");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceAbbreviationOverrideCommand.Abbreviation));
    }

    [Fact]
    public void Given_AbbreviationWithSpecialChars_When_Validate_Then_Fails()
    {
        var command = new SetResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", "ab-c");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceAbbreviationOverrideCommand.Abbreviation));
    }
}
