using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.VirtualNetworkAggregate.Entities;

public sealed class SubnetTests
{
    [Fact]
    public void Given_ServiceEndpointPattern_When_InspectingRegex_Then_UsesFiniteTimeout()
    {
        // Arrange
        var serviceEndpointPatternField = typeof(Subnet).GetField(
            "ServiceEndpointPattern",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var regex = serviceEndpointPatternField!.GetValue(null);

        // Assert
        regex.Should().BeOfType<Regex>().Which.MatchTimeout.Should().NotBe(Regex.InfiniteMatchTimeout);
    }
}
