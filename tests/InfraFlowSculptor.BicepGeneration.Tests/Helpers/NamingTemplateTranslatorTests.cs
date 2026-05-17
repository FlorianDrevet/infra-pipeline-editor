using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class NamingTemplateTranslatorTests
{
    [Theory]
    [InlineData("{name}", "'${name}'")]
    [InlineData("{resourceAbbr}", "'${resourceAbbr}'")]
    [InlineData("{resourceType}", "'${resourceType}'")]
    [InlineData("{suffix}", "'${env.envSuffix}'")]
    [InlineData("{prefix}", "'${env.envPrefix}'")]
    [InlineData("{env}", "'${env.envName}'")]
    [InlineData("{envShort}", "'${env.envShort}'")]
    [InlineData("{location}", "'${env.location}'")]
    public void Given_SinglePlaceholder_When_ToBicepInterpolation_Then_MapsCorrectly(
        string template, string expected)
    {
        // Act
        var result = NamingTemplateTranslator.ToBicepInterpolation(template);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_CombinedTemplate_When_ToBicepInterpolation_Then_MapsAllPlaceholders()
    {
        // Arrange
        const string template = "{name}-{resourceAbbr}{suffix}";

        // Act
        var result = NamingTemplateTranslator.ToBicepInterpolation(template);

        // Assert
        result.Should().Be("'${name}-${resourceAbbr}${env.envSuffix}'");
    }

    [Fact]
    public void Given_UnknownPlaceholder_When_ToBicepInterpolation_Then_PreservesAsInterpolation()
    {
        // Arrange
        const string template = "{customField}";

        // Act
        var result = NamingTemplateTranslator.ToBicepInterpolation(template);

        // Assert
        result.Should().Be("'${customField}'");
    }

    [Fact]
    public void Given_NoPlaceholders_When_ToBicepInterpolation_Then_ReturnsLiteralInQuotes()
    {
        // Arrange
        const string template = "my-static-name";

        // Act
        var result = NamingTemplateTranslator.ToBicepInterpolation(template);

        // Assert
        result.Should().Be("'my-static-name'");
    }

    [Fact]
    public void Given_CaseInsensitivePlaceholder_When_ToBicepInterpolation_Then_StillMaps()
    {
        // Arrange
        const string template = "{NAME}";

        // Act
        var result = NamingTemplateTranslator.ToBicepInterpolation(template);

        // Assert
        result.Should().Be("'${name}'");
    }

    [Theory]
    [InlineData("{name}-{resourceType}-{suffix}", true)]
    [InlineData("{RESOURCETYPE}", true)]
    [InlineData("{name}-{resourceAbbr}", false)]
    [InlineData("no-placeholders", false)]
    public void Given_Template_When_UsesResourceType_Then_DetectsCorrectly(
        string template, bool expected)
    {
        // Act
        var result = NamingTemplateTranslator.UsesResourceType(template);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("KeyVault", "BuildKeyVaultName")]
    [InlineData("StorageAccount", "BuildStorageAccountName")]
    [InlineData("ResourceGroup", "BuildResourceGroupName")]
    public void Given_ResourceType_When_GetFunctionName_Then_ReturnsBuildTypeName(
        string resourceType, string expected)
    {
        // Act
        var result = NamingTemplateTranslator.GetFunctionName(resourceType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_EmptyTemplate_When_ToBicepInterpolation_Then_ReturnsEmptyQuotedString()
    {
        // Act
        var result = NamingTemplateTranslator.ToBicepInterpolation(string.Empty);

        // Assert
        result.Should().Be("''");
    }
}
