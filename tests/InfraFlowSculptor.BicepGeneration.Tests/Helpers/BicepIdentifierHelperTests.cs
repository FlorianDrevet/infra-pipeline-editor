using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class BicepIdentifierHelperTests
{
    [Theory]
    [InlineData("my-rg-prod", "myRgProd")]
    [InlineData("storage_account", "storageAccount")]
    [InlineData("my resource name", "myResourceName")]
    [InlineData("simple", "simple")]
    [InlineData("ALLCAPS", "allcaps")]
    [InlineData("a-B-c", "aBC")]
    [InlineData("my--double-dash", "myDoubleDash")]
    public void Given_ResourceName_When_ToBicepIdentifier_Then_ReturnsCamelCase(
        string input, string expected)
    {
        // Act
        var result = BicepIdentifierHelper.ToBicepIdentifier(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyOrWhitespace_When_ToBicepIdentifier_Then_ReturnsFallback(string input)
    {
        // Act
        var result = BicepIdentifierHelper.ToBicepIdentifier(input);

        // Assert
        result.Should().Be("resource");
    }

    [Fact]
    public void Given_SingleCharName_When_ToBicepIdentifier_Then_ReturnsLowercased()
    {
        // Act
        var result = BicepIdentifierHelper.ToBicepIdentifier("X");

        // Assert
        result.Should().Be("x");
    }

    [Fact]
    public void Given_HyphenOnly_When_ToBicepIdentifier_Then_ReturnsFallback()
    {
        // Act
        var result = BicepIdentifierHelper.ToBicepIdentifier("---");

        // Assert
        result.Should().Be("resource");
    }
}
