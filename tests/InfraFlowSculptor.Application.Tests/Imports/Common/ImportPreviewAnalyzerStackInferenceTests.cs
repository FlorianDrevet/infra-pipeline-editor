using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common.Analysis;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Tests.Imports.Common;

public sealed class ImportPreviewAnalyzerStackInferenceTests
{
    private readonly ImportPreviewAnalyzer _sut = new();

    [Fact]
    public void Given_WebAppWithDotNetLinuxFxVersion_When_Analyze_Then_SuggestsDotNetStack()
    {
        // Arrange
        var armJson = BuildArmTemplate(AzureResourceTypes.ArmTypes.WebAppType, "myWebApp",
            """{"siteConfig": {"linuxFxVersion": "DOTNETCORE|8.0"}}""");

        // Act
        var result = _sut.AnalyzeArmTemplate(armJson);

        // Assert
        var resource = result.Resources.FirstOrDefault(r => r.SourceName == "myWebApp");
        resource.Should().NotBeNull();
        resource!.SuggestedApplicationStack.Should().Be("DotNet");
    }

    [Fact]
    public void Given_WebAppWithNodeLinuxFxVersion_When_Analyze_Then_SuggestsNodeJsStack()
    {
        // Arrange
        var armJson = BuildArmTemplate(AzureResourceTypes.ArmTypes.WebAppType, "myNodeApp",
            """{"siteConfig": {"linuxFxVersion": "NODE|20-lts"}}""");

        // Act
        var result = _sut.AnalyzeArmTemplate(armJson);

        // Assert
        var resource = result.Resources.FirstOrDefault(r => r.SourceName == "myNodeApp");
        resource.Should().NotBeNull();
        resource!.SuggestedApplicationStack.Should().Be("NodeJs");
    }

    [Fact]
    public void Given_WebAppWithPythonLinuxFxVersion_When_Analyze_Then_SuggestsPythonStack()
    {
        // Arrange
        var armJson = BuildArmTemplate(AzureResourceTypes.ArmTypes.WebAppType, "myPythonApp",
            """{"siteConfig": {"linuxFxVersion": "PYTHON|3.11"}}""");

        // Act
        var result = _sut.AnalyzeArmTemplate(armJson);

        // Assert
        var resource = result.Resources.FirstOrDefault(r => r.SourceName == "myPythonApp");
        resource.Should().NotBeNull();
        resource!.SuggestedApplicationStack.Should().Be("Python");
    }

    [Fact]
    public void Given_WebAppWithJavaLinuxFxVersion_When_Analyze_Then_SuggestsJavaStack()
    {
        // Arrange
        var armJson = BuildArmTemplate(AzureResourceTypes.ArmTypes.WebAppType, "myJavaApp",
            """{"siteConfig": {"linuxFxVersion": "JAVA|17-java17"}}""");

        // Act
        var result = _sut.AnalyzeArmTemplate(armJson);

        // Assert
        var resource = result.Resources.FirstOrDefault(r => r.SourceName == "myJavaApp");
        resource.Should().NotBeNull();
        resource!.SuggestedApplicationStack.Should().Be("Java");
    }

    [Fact]
    public void Given_KeyVault_When_Analyze_Then_SuggestedStackIsNull()
    {
        // Arrange
        var armJson = BuildArmTemplate(AzureResourceTypes.ArmTypes.KeyVaultType, "myKeyVault",
            """{"tenantId": "00000000-0000-0000-0000-000000000000", "sku": {"name": "standard", "family": "A"}}""");

        // Act
        var result = _sut.AnalyzeArmTemplate(armJson);

        // Assert
        var resource = result.Resources.FirstOrDefault(r => r.SourceName == "myKeyVault");
        resource.Should().NotBeNull();
        resource!.SuggestedApplicationStack.Should().BeNull();
    }

    [Fact]
    public void Given_WebAppWithNoLinuxFxVersion_When_Analyze_Then_SuggestedStackIsNull()
    {
        // Arrange
        var armJson = BuildArmTemplate(AzureResourceTypes.ArmTypes.WebAppType, "myApp",
            """{"serverFarmId": "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Web/serverfarms/plan"}""");

        // Act
        var result = _sut.AnalyzeArmTemplate(armJson);

        // Assert
        var resource = result.Resources.FirstOrDefault(r => r.SourceName == "myApp");
        resource.Should().NotBeNull();
        resource!.SuggestedApplicationStack.Should().BeNull();
    }

    private static string BuildArmTemplate(string resourceType, string resourceName, string properties)
    {
        return $$"""
        {
            "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
            "contentVersion": "1.0.0.0",
            "resources": [
                {
                    "type": "{{resourceType}}",
                    "apiVersion": "2023-01-01",
                    "name": "{{resourceName}}",
                    "location": "[resourceGroup().location]",
                    "properties": {{properties}}
                }
            ]
        }
        """;
    }
}
