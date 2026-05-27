using FluentAssertions;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.UpdateVirtualNetwork;

public sealed class UpdateVirtualNetworkCommandValidatorTests
{
    private readonly UpdateVirtualNetworkCommandValidator _sut = new();

    private static UpdateVirtualNetworkCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        new Name("my-vnet"),
        new Location(Location.LocationEnum.FranceCentral));

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        var command = ValidCommand() with { Id = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateVirtualNetworkCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateVirtualNetworkCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateVirtualNetworkCommand.Location));
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
