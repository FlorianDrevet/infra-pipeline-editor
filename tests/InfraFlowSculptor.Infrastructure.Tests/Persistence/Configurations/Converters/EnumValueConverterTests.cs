using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class EnumValueConverterTests
{
    private readonly EnumValueConverter<Location, Location.LocationEnum> _sut = new();

    [Theory]
    [InlineData(Location.LocationEnum.WestEurope)]
    [InlineData(Location.LocationEnum.FranceCentral)]
    [InlineData(Location.LocationEnum.EastUS)]
    public void ConvertToProvider_Given_Location_Should_Return_EnumName(Location.LocationEnum enumValue)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var location = new Location(enumValue);

        // Act
        var result = toProvider(location);

        // Assert
        result.Should().Be(enumValue.ToString());
    }

    [Theory]
    [InlineData("WestEurope", Location.LocationEnum.WestEurope)]
    [InlineData("FranceCentral", Location.LocationEnum.FranceCentral)]
    [InlineData("EastUS", Location.LocationEnum.EastUS)]
    public void ConvertFromProvider_Given_String_Should_Return_Location(string raw, Location.LocationEnum expected)
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider(raw);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(Location.LocationEnum.WestEurope)]
    [InlineData(Location.LocationEnum.FranceCentral)]
    [InlineData(Location.LocationEnum.NorthEurope)]
    [InlineData(Location.LocationEnum.UKSouth)]
    public void RoundTrip_Given_Location_Should_Return_EqualLocation(Location.LocationEnum enumValue)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var original = new Location(enumValue);

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }
}
