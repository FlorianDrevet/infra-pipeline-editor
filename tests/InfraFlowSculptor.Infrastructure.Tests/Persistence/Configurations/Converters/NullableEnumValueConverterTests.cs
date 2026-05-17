using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class NullableEnumValueConverterTests
{
    private readonly NullableEnumValueConverter<Location, Location.LocationEnum> _sut = new();

    [Theory]
    [InlineData(Location.LocationEnum.WestEurope)]
    [InlineData(Location.LocationEnum.FranceCentral)]
    public void ConvertToProvider_Given_NonNullLocation_Should_Return_EnumName(Location.LocationEnum enumValue)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        Location? location = new Location(enumValue);

        // Act
        var result = toProvider(location);

        // Assert
        result.Should().Be(enumValue.ToString());
    }

    [Fact]
    public void ConvertToProvider_Given_Null_Should_Return_Null()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();

        // Act
        var result = toProvider(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("WestEurope", Location.LocationEnum.WestEurope)]
    [InlineData("FranceCentral", Location.LocationEnum.FranceCentral)]
    public void ConvertFromProvider_Given_ValidString_Should_Return_Location(string raw, Location.LocationEnum expected)
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider(raw);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromProvider_Given_Null_Should_Return_Null()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ConvertFromProvider_Given_EmptyString_Should_Return_Null()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ConvertFromProvider_Given_WhitespaceString_Should_Return_Null()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider("   ");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(Location.LocationEnum.WestEurope)]
    [InlineData(Location.LocationEnum.EastUS)]
    public void RoundTrip_Given_NonNullLocation_Should_Return_EqualLocation(Location.LocationEnum enumValue)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        Location? original = new Location(enumValue);

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_Given_Null_Should_Return_Null()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var dbValue = toProvider(null);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().BeNull();
    }
}
