using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.TextManipulation;

namespace InfraFlowSculptor.BicepGeneration.Tests.TextManipulation;

public sealed class BicepTagsInjectorTests
{
    private const string MinimalModule = """
        @description('Location for all resources')
        param location string

        resource kv 'Microsoft.KeyVault/vaults' = {
          name: 'myKv'
          location: location
          properties: {
            sku: { family: 'A' }
          }
        }
        """;

    [Fact]
    public void Given_ModuleWithoutTags_When_Inject_Then_AddsTagsParamAndProperty()
    {
        // Act
        var result = BicepTagsInjector.Inject(MinimalModule);

        // Assert
        result.Should().Contain("param tags object = {}");
        result.Should().Contain("tags: tags");
    }

    [Fact]
    public void Given_ModuleWithoutTags_When_Inject_Then_TagsParamHasDescription()
    {
        // Act
        var result = BicepTagsInjector.Inject(MinimalModule);

        // Assert
        result.Should().Contain("@description('Resource tags')");
    }

    [Fact]
    public void Given_ModuleWithExistingTagsParam_When_Inject_Then_NoOp()
    {
        // Arrange
        const string bicep = """
            param location string
            param tags object = {}

            resource kv 'Microsoft.KeyVault/vaults' = {
              name: 'myKv'
              location: location
              properties: {}
            }
            """;

        // Act
        var result = BicepTagsInjector.Inject(bicep);

        // Assert
        result.Should().Be(bicep);
    }

    [Fact]
    public void Given_ModuleWithLocationProperty_When_Inject_Then_TagsPropertyInsertedAfterLocation()
    {
        // Act
        var result = BicepTagsInjector.Inject(MinimalModule);

        // Assert
        var locationIdx = result.IndexOf("location: location", StringComparison.Ordinal);
        var tagsPropertyIdx = result.IndexOf("tags: tags", StringComparison.Ordinal);
        tagsPropertyIdx.Should().BeGreaterThan(locationIdx);
    }

    [Fact]
    public void Given_ModuleWithMultipleParams_When_Inject_Then_TagsParamAddedAfterLastParam()
    {
        // Arrange
        const string bicep = """
            param location string
            param sku string

            resource kv 'Microsoft.KeyVault/vaults' = {
              name: 'myKv'
              location: location
              properties: {}
            }
            """;

        // Act
        var result = BicepTagsInjector.Inject(bicep);

        // Assert
        var lastOriginalParam = result.IndexOf("param sku", StringComparison.Ordinal);
        var tagsParam = result.IndexOf("param tags", StringComparison.Ordinal);
        tagsParam.Should().BeGreaterThan(lastOriginalParam);
    }
}
