using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class SingleValueConverterTests
{
    private readonly SingleValueConverter<Name, string> _sut = new();

    [Fact]
    public void ConvertToProvider_Given_Name_Should_Return_UnderlyingString()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var name = new Name("test-value");

        // Act
        var result = toProvider(name);

        // Assert
        result.Should().Be("test-value");
    }

    [Fact]
    public void ConvertFromProvider_Given_String_Should_Return_Name()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider("test-value");

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("test-value");
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("my-project-name")]
    [InlineData("X")]
    public void RoundTrip_Given_Name_Should_Return_EqualName(string raw)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var original = new Name(raw);

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }
}
