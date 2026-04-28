using System.Text.Json;
using FluentAssertions;
using InfraFlowSculptor.Mcp.Imports;
using InfraFlowSculptor.Mcp.Imports.Models;

namespace InfraFlowSculptor.Mcp.Tests.Imports;

public sealed class ImportPreviewServiceTests
{
    private readonly ImportPreviewService _sut = new();

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
    public void CreatePreviewFromArm_ValidTemplate_ParsesResources()
    {
        // Act
        var preview = _sut.CreatePreviewFromArm(SingleKeyVaultArm);

        // Assert
        preview.Should().NotBeNull();
        preview.PreviewId.Should().StartWith("preview_");
        preview.ProjectDefinition.SourceFormat.Should().Be("arm-json");
        preview.ProjectDefinition.Resources.Should().HaveCount(1);

        var resource = preview.ProjectDefinition.Resources[0];
        resource.SourceType.Should().Be("Microsoft.KeyVault/vaults");
        resource.SourceName.Should().Be("myKeyVault");
        resource.MappedResourceType.Should().Be("KeyVault");
        resource.Confidence.Should().Be(MappingConfidence.High);
    }

    [Fact]
    public void CreatePreviewFromArm_UnsupportedResourceType_MarkedAsUnsupported()
    {
        // Arrange
        var arm = """
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
        var preview = _sut.CreatePreviewFromArm(arm);

        // Assert
        preview.ProjectDefinition.Resources.Should().HaveCount(1);
        var resource = preview.ProjectDefinition.Resources[0];
        resource.MappedResourceType.Should().BeNull();
        preview.UnsupportedResources.Should().Contain("fakeResource");
        preview.Gaps.Should().Contain(g =>
            g.Category == "unsupported_resource" &&
            g.Severity == ImportGapSeverity.Warning);
    }

    [Fact]
    public void CreatePreviewFromArm_ExtractsKeyVaultSku()
    {
        // Act
        var preview = _sut.CreatePreviewFromArm(SingleKeyVaultArm);

        // Assert
        var resource = preview.ProjectDefinition.Resources[0];
        resource.ExtractedProperties.Should().ContainKey("skuName");
        resource.ExtractedProperties["skuName"].Should().Be("standard");
    }

    [Fact]
    public void CreatePreviewFromArm_MultipleResources_AllParsed()
    {
        // Act
        var preview = _sut.CreatePreviewFromArm(MultiResourceArm);

        // Assert
        preview.ProjectDefinition.Resources.Should().HaveCount(2);
        preview.ProjectDefinition.Resources.Should().Contain(r => r.MappedResourceType == "KeyVault");
        preview.ProjectDefinition.Resources.Should().Contain(r => r.MappedResourceType == "StorageAccount");
    }

    [Fact]
    public void CreatePreviewFromArm_InvalidJson_ThrowsJsonException()
    {
        // Act
        var act = () => _sut.CreatePreviewFromArm("not valid json {{{");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void CreatePreviewFromArm_NoResourcesArray_HandlesGracefully()
    {
        // Arrange
        var arm = """{ "parameters": {} }""";

        // Act
        var preview = _sut.CreatePreviewFromArm(arm);

        // Assert
        preview.ProjectDefinition.Resources.Should().BeEmpty();
        preview.UnsupportedResources.Should().BeEmpty();
    }

    [Fact]
    public void CreatePreviewFromArm_DependsOn_CreatesDependencies()
    {
        // Act
        var preview = _sut.CreatePreviewFromArm(MultiResourceArm);

        // Assert
        preview.ProjectDefinition.Dependencies.Should().HaveCount(1);
        var dep = preview.ProjectDefinition.Dependencies[0];
        dep.FromResourceName.Should().Be("mystorageaccount");
        dep.ToResourceName.Should().Contain("myKeyVault");
        dep.DependencyType.Should().Be("dependsOn");
    }

    [Fact]
    public void GetPreview_ExistingId_ReturnsPreview()
    {
        // Arrange
        var preview = _sut.CreatePreviewFromArm(SingleKeyVaultArm);

        // Act
        var retrieved = _sut.GetPreview(preview.PreviewId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.PreviewId.Should().Be(preview.PreviewId);
    }

    [Fact]
    public void GetPreview_NonExistentId_ReturnsNull()
    {
        // Act
        var retrieved = _sut.GetPreview("preview_nonexistent");

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact]
    public void RemovePreview_ExistingId_ReturnsTrue()
    {
        // Arrange
        var preview = _sut.CreatePreviewFromArm(SingleKeyVaultArm);

        // Act
        var removed = _sut.RemovePreview(preview.PreviewId);

        // Assert
        removed.Should().BeTrue();
        _sut.GetPreview(preview.PreviewId).Should().BeNull();
    }
}
