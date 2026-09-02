using FluentAssertions;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateSubnet;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Xunit;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.UpdateSubnet;

public sealed class UpdateSubnetCommandValidatorTests
{
    private readonly UpdateSubnetCommandValidator _sut = new();

    private static UpdateSubnetCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        Guid.NewGuid(),
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
        var command = ValidCommand() with { Delegation = "PostgresFlexible" };

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
        var command = ValidCommand() with { Delegation = "NotARealDelegation" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.Delegation));
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
        var command = ValidCommand() with { PrivateEndpointNetworkPolicies = "NotAPolicy" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.PrivateEndpointNetworkPolicies));
    }

    [Fact]
    public void Given_EmptyVirtualNetworkId_When_Validate_Then_FailsOnVirtualNetworkId()
    {
        var command = ValidCommand() with { VirtualNetworkId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.VirtualNetworkId));
    }

    [Fact]
    public void Given_EmptySubnetId_When_Validate_Then_FailsOnSubnetId()
    {
        var command = ValidCommand() with { SubnetId = Guid.Empty };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.SubnetId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.Name));
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = new string('a', 81) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.Name));
    }

    [Fact]
    public void Given_InvalidAddressPrefix_When_Validate_Then_FailsOnAddressPrefix()
    {
        var command = ValidCommand() with { AddressPrefix = "not-a-cidr" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.AddressPrefix));
    }

    [Fact]
    public void Given_EmptyPrivateEndpointNetworkPolicies_When_Validate_Then_FailsOnPolicy()
    {
        var command = ValidCommand() with { PrivateEndpointNetworkPolicies = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSubnetCommand.PrivateEndpointNetworkPolicies));
    }
}
