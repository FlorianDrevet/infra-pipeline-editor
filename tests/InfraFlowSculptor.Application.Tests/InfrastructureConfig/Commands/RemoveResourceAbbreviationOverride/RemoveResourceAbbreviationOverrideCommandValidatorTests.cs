using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceAbbreviationOverride;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.RemoveResourceAbbreviationOverride;

public sealed class RemoveResourceAbbreviationOverrideCommandValidatorTests
{
    private readonly RemoveResourceAbbreviationOverrideCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new RemoveResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfraConfigId_When_Validate_Then_Fails()
    {
        var command = new RemoveResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.Create(Guid.Empty), "Microsoft.KeyVault/vaults");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InfraConfigId.Value");
    }

    [Fact]
    public void Given_EmptyResourceType_When_Validate_Then_Fails()
    {
        var command = new RemoveResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveResourceAbbreviationOverrideCommand.ResourceType));
    }

    [Fact]
    public void Given_ResourceTypeTooLong_When_Validate_Then_Fails()
    {
        var command = new RemoveResourceAbbreviationOverrideCommand(
            InfrastructureConfigId.CreateUnique(), new string('x', 101));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveResourceAbbreviationOverrideCommand.ResourceType));
    }
}
