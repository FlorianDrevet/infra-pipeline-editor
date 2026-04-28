using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class BicepFormattingHelperTests
{
    [Theory]
    [InlineData("storageAccount")]
    [InlineData("_sharedType")]
    [InlineData("key1")]
    public void Given_ValidBicepObjectKey_When_Formatting_Then_ReturnsUnquotedIdentifier(string key)
    {
        // Act
        var result = BicepFormattingHelper.FormatBicepObjectKey(key);

        // Assert
        result.Should().Be(key);
    }

    [Theory]
    [InlineData("storage-account", "'storage-account'")]
    [InlineData("storage account", "'storage account'")]
    [InlineData("o'clock", "'o\\'clock'")]
    public void Given_InvalidBicepObjectKey_When_Formatting_Then_ReturnsQuotedEscapedValue(string key, string expected)
    {
        // Act
        var result = BicepFormattingHelper.FormatBicepObjectKey(key);

        // Assert
        result.Should().Be(expected);
    }
}