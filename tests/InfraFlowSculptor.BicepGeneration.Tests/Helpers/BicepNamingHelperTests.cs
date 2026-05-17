using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class BicepNamingHelperTests
{
    [Fact]
    public void Given_ResourceTypeOverride_When_BuildNamingExpression_Then_ReturnsTypedFunctionCall()
    {
        // Arrange
        var context = new NamingContext
        {
            ResourceTemplates = new Dictionary<string, string>
            {
                [AzureResourceTypes.KeyVault] = "{name}-{resourceAbbr}{suffix}"
            }
        };

        // Act
        var result = BicepNamingHelper.BuildNamingExpression(
            "my-kv", "kv", AzureResourceTypes.KeyVault, context);

        // Assert
        result.Should().Be("BuildKeyVaultName('my-kv', 'kv', env)");
    }

    [Fact]
    public void Given_DefaultTemplateOnly_When_BuildNamingExpression_Then_ReturnsGenericFunctionCall()
    {
        // Arrange
        var context = new NamingContext
        {
            DefaultTemplate = "{name}-{resourceAbbr}{suffix}"
        };

        // Act
        var result = BicepNamingHelper.BuildNamingExpression(
            "my-storage", "stg", AzureResourceTypes.StorageAccount, context);

        // Assert
        result.Should().Be("BuildResourceName('my-storage', 'stg', env)");
    }

    [Fact]
    public void Given_NoTemplate_When_BuildNamingExpression_Then_ReturnsLiteralName()
    {
        // Arrange
        var context = new NamingContext();

        // Act
        var result = BicepNamingHelper.BuildNamingExpression(
            "my-resource", "res", AzureResourceTypes.WebApp, context);

        // Assert
        result.Should().Be("'my-resource'");
    }

    [Fact]
    public void Given_ContextWithOverrides_When_BuildFunctionImportList_Then_ContainsTypedFunctions()
    {
        // Arrange
        var context = new NamingContext
        {
            ResourceTemplates = new Dictionary<string, string>
            {
                [AzureResourceTypes.KeyVault] = "{name}-kv"
            }
        };

        var modules = new List<GeneratedTypeModule>
        {
            new() { ResourceTypeName = AzureResourceTypes.KeyVault }
        };

        var resourceGroups = new List<ResourceGroupDefinition>
        {
            new() { Name = "rg-app" }
        };

        // Act
        var result = BicepNamingHelper.BuildFunctionImportList(context, modules, resourceGroups);

        // Assert
        result.Should().Contain("BuildKeyVaultName");
    }

    [Fact]
    public void Given_DefaultTemplateWithNoOverrides_When_BuildFunctionImportList_Then_ContainsBuildResourceName()
    {
        // Arrange
        var context = new NamingContext
        {
            DefaultTemplate = "{name}-{resourceAbbr}"
        };

        var modules = new List<GeneratedTypeModule>
        {
            new() { ResourceTypeName = AzureResourceTypes.StorageAccount }
        };

        var resourceGroups = new List<ResourceGroupDefinition>
        {
            new() { Name = "rg-app" }
        };

        // Act
        var result = BicepNamingHelper.BuildFunctionImportList(context, modules, resourceGroups);

        // Assert
        result.Should().Contain("BuildResourceName");
    }

    [Fact]
    public void Given_NoTemplates_When_BuildFunctionImportList_Then_ReturnsEmpty()
    {
        // Arrange
        var context = new NamingContext();
        var modules = new List<GeneratedTypeModule>();
        var resourceGroups = new List<ResourceGroupDefinition>();

        // Act
        var result = BicepNamingHelper.BuildFunctionImportList(context, modules, resourceGroups);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("MY_API_URL", "MyApiUrl")]
    [InlineData("APP__HOST", "AppHost")]
    [InlineData("simple", "Simple")]
    [InlineData("SINGLE", "Single")]
    [InlineData("a-b-c", "ABC")]
    [InlineData("", "")]
    public void Given_EnvVarName_When_ToPascalCaseFromEnvVar_Then_ReturnsPascalCase(
        string input, string expected)
    {
        // Act
        var result = BicepNamingHelper.ToPascalCaseFromEnvVar(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("kv.properties.vaultUri", "properties.vaultUri")]
    [InlineData("stg.properties.primaryEndpoints.blob", "properties.primaryEndpoints.blob")]
    [InlineData("singleIdentifier", "singleIdentifier")]
    [InlineData(null, null)]
    public void Given_BicepExpression_When_StripResourceSymbolPrefix_Then_ReturnsStripped(
        string? input, string? expected)
    {
        // Act
        var result = BicepNamingHelper.StripResourceSymbolPrefix(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_ExpressionWithInterpolation_When_StripResourceSymbolPrefix_Then_ReturnsOriginal()
    {
        // Arrange
        const string expression = "'${kv.name}'.something";

        // Act
        var result = BicepNamingHelper.StripResourceSymbolPrefix(expression);

        // Assert
        result.Should().Be(expression);
    }
}
