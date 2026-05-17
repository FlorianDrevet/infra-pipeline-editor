using FluentAssertions;

namespace InfraFlowSculptor.GenerationCore.Tests;

public sealed class BicepIdentifierNormalizerTests
{
    [Theory]
    [InlineData("my-rg-prod", "resource", "myRgProd")]
    [InlineData("My_RG prod", "resource", "myRgProd")]
    [InlineData("alreadyCamel", "resource", "alreadycamel")]
    [InlineData("", "resource", "resource")]
    [InlineData("___", "unknown", "unknown")]
    public void Given_InputAndFallback_When_NormalizingCamelCase_Then_ReturnsCanonicalIdentifier(
        string value,
        string fallbackValue,
        string expected)
    {
        // Act
        var result = BicepIdentifierNormalizer.NormalizeCamelCase(value, fallbackValue);

        // Assert
        result.Should().Be(expected);
    }
}
