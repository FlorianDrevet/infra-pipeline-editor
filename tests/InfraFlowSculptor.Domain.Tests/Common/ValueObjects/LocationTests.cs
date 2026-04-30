using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.Common.ValueObjects;

public sealed class LocationTests
{
    [Fact]
    public void Given_WestEurope_When_GettingDefaultAzureRegionKey_Then_ReturnsWireFormat()
    {
        // Act
        var result = Location.DefaultAzureRegionKey;

        // Assert
        result.Should().Be("westeurope");
    }

    [Fact]
    public void Given_FranceCentral_When_ConvertingEnumToAzureRegionKey_Then_ReturnsWireFormat()
    {
        // Act
        var result = Location.ToAzureRegionKey(Location.LocationEnum.FranceCentral);

        // Assert
        result.Should().Be("francecentral");
    }
}