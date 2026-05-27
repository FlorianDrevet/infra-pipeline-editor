using FluentAssertions;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.CreateVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.CreateVirtualNetwork;

public sealed class CreateVirtualNetworkCommandValidatorTests
{
    private readonly CreateVirtualNetworkCommandValidator _sut = new();

    private static CreateVirtualNetworkCommand ValidCommand() => new(
        ResourceGroupId.CreateUnique(),
        new Name("my-vnet"),
        new Location(Location.LocationEnum.FranceCentral));

    private static CreateVirtualNetworkCommand ValidCommandWithEnvSettings() => new(
        ResourceGroupId.CreateUnique(),
        new Name("my-vnet"),
        new Location(Location.LocationEnum.FranceCentral),
        EnvironmentSettings:
        [
            new VirtualNetworkEnvironmentConfigData("dev", ["10.0.0.0/16"], ["10.0.0.4"])
        ]);

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidCommandWithEnvironmentSettings_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommandWithEnvSettings());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        var command = ValidCommand() with { ResourceGroupId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateVirtualNetworkCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateVirtualNetworkCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateVirtualNetworkCommand.Location));
    }

    [Fact]
    public void Given_EnvironmentSettingsWithEmptyEnvironmentName_When_Validate_Then_Fails()
    {
        var command = ValidCommand() with
        {
            EnvironmentSettings = [new VirtualNetworkEnvironmentConfigData("", ["10.0.0.0/16"], null)]
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("EnvironmentName"));
    }

    [Fact]
    public void Given_EnvironmentSettingsWithEmptyAddressSpaces_When_Validate_Then_Fails()
    {
        var command = ValidCommand() with
        {
            EnvironmentSettings = [new VirtualNetworkEnvironmentConfigData("dev", [], null)]
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("AddressSpaces"));
    }

    [Fact]
    public void Given_EnvironmentSettingsWithInvalidCidr_When_Validate_Then_Fails()
    {
        var command = ValidCommand() with
        {
            EnvironmentSettings = [new VirtualNetworkEnvironmentConfigData("dev", ["not-cidr"], null)]
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("AddressSpaces"));
    }

    [Fact]
    public void Given_EnvironmentSettingsWithInvalidDnsServer_When_Validate_Then_Fails()
    {
        var command = ValidCommand() with
        {
            EnvironmentSettings = [new VirtualNetworkEnvironmentConfigData("dev", ["10.0.0.0/16"], ["not-ip"])]
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DnsServers"));
    }
}
