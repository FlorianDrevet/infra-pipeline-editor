using FluentAssertions;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.AddSubnet;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Xunit;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.AddSubnet;

public sealed class AddSubnetCommandValidatorTests
{
    private readonly AddSubnetCommandValidator _sut = new();

    private static AddSubnetCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        "subnet-app",
        "10.0.1.0/24",
        Delegation: null,
        ServiceEndpoints: null,
        PrivateEndpointNetworkPolicies: "Disabled",
        NsgId: null);

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidDelegation_When_Validate_Then_Succeeds()
    {
        var command = ValidCommand() with { Delegation = "WebServerFarms" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullDelegation_When_Validate_Then_Succeeds()
    {
        var command = ValidCommand() with { Delegation = null };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_InvalidDelegation_When_Validate_Then_FailsOnDelegation()
    {
        var command = ValidCommand() with { Delegation = "InvalidDelegationValue" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddSubnetCommand.Delegation));
    }

    [Fact]
    public void Given_ValidPrivateEndpointNetworkPolicies_When_Validate_Then_Succeeds()
    {
        foreach (var policy in new[] { "Disabled", "Enabled", "NetworkSecurityGroupEnabled", "RouteTableEnabled" })
        {
            var command = ValidCommand() with { PrivateEndpointNetworkPolicies = policy };

            var result = _sut.Validate(command);

            result.IsValid.Should().BeTrue($"policy '{policy}' should be valid");
        }
    }

    [Fact]
    public void Given_InvalidPrivateEndpointNetworkPolicies_When_Validate_Then_FailsOnPolicy()
    {
        var command = ValidCommand() with { PrivateEndpointNetworkPolicies = "InvalidPolicy" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddSubnetCommand.PrivateEndpointNetworkPolicies));
    }

    [Fact]
    public void Given_EmptyVirtualNetworkId_When_Validate_Then_FailsOnVirtualNetworkId()
    {
        var command = ValidCommand() with { VirtualNetworkId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddSubnetCommand.VirtualNetworkId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddSubnetCommand.Name));
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = new string('a', 81) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddSubnetCommand.Name));
    }

    [Fact]
    public void Given_InvalidAddressPrefix_When_Validate_Then_FailsOnAddressPrefix()
    {
        var command = ValidCommand() with { AddressPrefix = "not-a-cidr" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddSubnetCommand.AddressPrefix));
    }

    [Fact]
    public void Given_EmptyPrivateEndpointNetworkPolicies_When_Validate_Then_FailsOnPolicy()
    {
        var command = ValidCommand() with { PrivateEndpointNetworkPolicies = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddSubnetCommand.PrivateEndpointNetworkPolicies));
    }
}
