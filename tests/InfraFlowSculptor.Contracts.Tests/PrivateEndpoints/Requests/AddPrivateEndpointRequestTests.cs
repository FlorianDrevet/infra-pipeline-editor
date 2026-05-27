using FluentAssertions;
using InfraFlowSculptor.Contracts.PrivateEndpoints.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.PrivateEndpoints.Requests;

public sealed class AddPrivateEndpointRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddPrivateEndpointRequest
        {
            SubnetId = Guid.NewGuid(),
            GroupId = "vault",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptySubnetId_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddPrivateEndpointRequest
        {
            SubnetId = Guid.Empty,
            GroupId = "vault",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddPrivateEndpointRequest.SubnetId)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullGroupId_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddPrivateEndpointRequest
        {
            SubnetId = Guid.NewGuid(),
            GroupId = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddPrivateEndpointRequest.GroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_GroupIdOver100Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddPrivateEndpointRequest
        {
            SubnetId = Guid.NewGuid(),
            GroupId = new string('x', 101),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddPrivateEndpointRequest.GroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_PrivateDnsZoneIdEmpty_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddPrivateEndpointRequest
        {
            SubnetId = Guid.NewGuid(),
            GroupId = "vault",
            PrivateDnsZoneId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddPrivateEndpointRequest.PrivateDnsZoneId)).Should().BeTrue();
    }

    [Fact]
    public void Given_CustomNetworkInterfaceNameOver80Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddPrivateEndpointRequest
        {
            SubnetId = Guid.NewGuid(),
            GroupId = "vault",
            CustomNetworkInterfaceName = new string('x', 81),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddPrivateEndpointRequest.CustomNetworkInterfaceName)).Should().BeTrue();
    }
}
