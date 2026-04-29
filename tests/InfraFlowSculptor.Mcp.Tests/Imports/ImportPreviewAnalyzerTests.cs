using System.Text.Json;
using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common;

namespace InfraFlowSculptor.Mcp.Tests.Imports;

public sealed class ImportPreviewAnalyzerTests
{
    private readonly ImportPreviewAnalyzer _sut = new();

    private const string SingleKeyVaultArm = """
        {
          "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
          "contentVersion": "1.0.0.0",
          "resources": [
            {
              "type": "Microsoft.KeyVault/vaults",
              "apiVersion": "2023-07-01",
              "name": "myKeyVault",
              "location": "[resourceGroup().location]",
              "properties": {
                "sku": {
                  "family": "A",
                  "name": "standard"
                },
                "tenantId": "[subscription().tenantId]"
              }
            }
          ]
        }
        """;

    private const string MultiResourceArm = """
        {
          "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
          "contentVersion": "1.0.0.0",
          "resources": [
            {
              "type": "Microsoft.KeyVault/vaults",
              "apiVersion": "2023-07-01",
              "name": "myKeyVault",
              "location": "[resourceGroup().location]",
              "properties": {
                "sku": { "family": "A", "name": "standard" },
                "tenantId": "[subscription().tenantId]"
              }
            },
            {
              "type": "Microsoft.Storage/storageAccounts",
              "apiVersion": "2023-01-01",
              "name": "mystorageaccount",
              "location": "[resourceGroup().location]",
              "kind": "StorageV2",
              "sku": { "name": "Standard_LRS" },
              "properties": {},
              "dependsOn": ["[resourceId('Microsoft.KeyVault/vaults', 'myKeyVault')]"]
            }
          ]
        }
        """;

    [Fact]
    public void Given_ValidArmTemplate_When_AnalyzeArmTemplate_Then_ReturnsMappedResources()
    {
        // Act
        var result = _sut.AnalyzeArmTemplate(SingleKeyVaultArm);

        // Assert
        result.SourceFormat.Should().Be(IacSourceFormat.ArmJson);
        result.Resources.Should().HaveCount(1);
        result.UnsupportedResources.Should().BeEmpty();

        var resource = result.Resources[0];
        resource.SourceType.Should().Be("Microsoft.KeyVault/vaults");
        resource.SourceName.Should().Be("myKeyVault");
        resource.MappedResourceType.Should().Be("KeyVault");
        resource.Confidence.Should().Be(ImportPreviewMappingConfidence.High);
        resource.ExtractedProperties["skuName"].Should().Be("standard");
    }

    [Fact]
    public void Given_UnsupportedResourceType_When_AnalyzeArmTemplate_Then_ReturnsUnsupportedGap()
    {
        // Arrange
        const string arm = """
            {
              "resources": [
                {
                  "type": "Microsoft.FakeProvider/fakeResources",
                  "apiVersion": "2023-01-01",
                  "name": "fakeResource",
                  "location": "westeurope",
                  "properties": {}
                }
              ]
            }
            """;

        // Act
        var result = _sut.AnalyzeArmTemplate(arm);

        // Assert
        result.Resources.Should().HaveCount(1);
        result.Resources[0].MappedResourceType.Should().BeNull();
        result.UnsupportedResources.Should().Contain("fakeResource");
        result.Gaps.Should().Contain(gap =>
            gap.Category == ImportPreviewGapCategory.UnsupportedResource &&
            gap.Severity == ImportPreviewGapSeverity.Warning &&
            gap.SourceResourceName == "fakeResource");
    }

    [Fact]
    public void Given_InvalidJson_When_AnalyzeArmTemplate_Then_ThrowsJsonException()
    {
        // Act
        var act = () => _sut.AnalyzeArmTemplate("not valid json {{{");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Given_TemplateMetadata_When_AnalyzeArmTemplate_Then_ReturnsMetadata()
    {
        // Act
        var result = _sut.AnalyzeArmTemplate(SingleKeyVaultArm);

        // Assert
        result.Metadata.Should().ContainKey("schema");
        result.Metadata.Should().ContainKey("contentVersion");
        result.Metadata["contentVersion"].Should().Be("1.0.0.0");
    }

    [Fact]
    public void Given_DependsOnEntries_When_AnalyzeArmTemplate_Then_ReturnsDependencies()
    {
        // Act
        var result = _sut.AnalyzeArmTemplate(MultiResourceArm);

        // Assert
        result.Dependencies.Should().ContainSingle();

        var dependency = result.Dependencies[0];
        dependency.FromResourceName.Should().Be("mystorageaccount");
        dependency.ToResourceName.Should().Be("myKeyVault");
        dependency.DependencyType.Should().Be(ImportDependencyType.DependsOn);
    }
}