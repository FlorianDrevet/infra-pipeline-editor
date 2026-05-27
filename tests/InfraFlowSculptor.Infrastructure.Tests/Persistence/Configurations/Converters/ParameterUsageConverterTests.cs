using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class ParameterUsageConverterTests
{
    private readonly ParameterUsageConverter _sut = new();

    [Theory]
    [InlineData("secret")]
    [InlineData("appSetting")]
    [InlineData("connectionString")]
    public void ConvertToProvider_Given_ParameterUsage_Should_Return_UnderlyingString(string raw)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var usage = ParameterUsage.From(raw);

        // Act
        var result = toProvider(usage);

        // Assert
        result.Should().Be(raw);
    }

    [Theory]
    [InlineData("secret")]
    [InlineData("appSetting")]
    [InlineData("connectionString")]
    public void ConvertFromProvider_Given_String_Should_Return_ParameterUsage(string raw)
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider(raw);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("secret")]
    [InlineData("appSetting")]
    [InlineData("connectionString")]
    public void RoundTrip_Given_ParameterUsage_Should_Return_EqualUsage(string raw)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var original = ParameterUsage.From(raw);

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }
}
