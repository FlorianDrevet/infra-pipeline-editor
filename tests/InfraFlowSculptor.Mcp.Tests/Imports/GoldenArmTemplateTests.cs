using System.Text.Json;
using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common.Analysis;
using InfraFlowSculptor.Application.Imports.Common.Constants;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Mcp.Tests.Imports;

/// <summary>
/// Golden tests that parse representative ARM templates end-to-end through
/// <see cref="ImportPreviewService"/> and verify the resulting preview structure.
/// Phase 4.8 of the MCP V1 implementation plan.
/// </summary>
public sealed class GoldenArmTemplateTests
{
  private readonly ImportPreviewAnalyzer _sut = new();

    // ── Template 1: Simple — Key Vault + Storage Account ──────────────

    private const string SimpleTemplate = """
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
            "tenantId": "[subscription().tenantId]",
            "enableSoftDelete": true
          }
        },
        {
          "type": "Microsoft.Storage/storageAccounts",
          "apiVersion": "2023-05-01",
          "name": "mystorageacct",
          "location": "[resourceGroup().location]",
          "kind": "StorageV2",
          "sku": { "name": "Standard_LRS" },
          "properties": {
            "supportsHttpsTrafficOnly": true,
            "minimumTlsVersion": "TLS1_2"
          }
        }
      ]
    }
    """;

    [Fact]
    public void SimpleTemplate_ParsesAllResources()
    {
      var preview = _sut.AnalyzeArmTemplate(SimpleTemplate);

      preview.Resources.Should().HaveCount(2);
        preview.UnsupportedResources.Should().BeEmpty();
    }

    [Fact]
    public void SimpleTemplate_MapsKeyVaultCorrectly()
    {
      var preview = _sut.AnalyzeArmTemplate(SimpleTemplate);

      var kv = preview.Resources
            .Single(r => r.MappedResourceType == AzureResourceTypes.KeyVault);

        kv.SourceType.Should().Be("Microsoft.KeyVault/vaults");
        kv.SourceName.Should().Be("myKeyVault");
      kv.Confidence.Should().Be(ImportPreviewMappingConfidence.High);
        kv.ExtractedProperties.Should().ContainKey("skuName")
            .WhoseValue.Should().Be("standard");
    }

    [Fact]
    public void SimpleTemplate_MapsStorageAccountCorrectly()
    {
      var preview = _sut.AnalyzeArmTemplate(SimpleTemplate);

      var sa = preview.Resources
            .Single(r => r.MappedResourceType == AzureResourceTypes.StorageAccount);

        sa.SourceType.Should().Be("Microsoft.Storage/storageAccounts");
        sa.SourceName.Should().Be("mystorageacct");
      sa.Confidence.Should().Be(ImportPreviewMappingConfidence.High);
        sa.ExtractedProperties.Should().ContainKey("kind")
            .WhoseValue.Should().Be("StorageV2");
        sa.ExtractedProperties.Should().ContainKey("skuName")
            .WhoseValue.Should().Be("Standard_LRS");
    }

    [Fact]
    public void SimpleTemplate_CapturesMetadata()
    {
      var preview = _sut.AnalyzeArmTemplate(SimpleTemplate);

      preview.Metadata.Should().ContainKey("schema");
      preview.Metadata.Should().ContainKey("contentVersion");
      preview.Metadata["contentVersion"].Should().Be("1.0.0.0");
    }

    [Fact]
    public void SimpleTemplate_GeneratesUnmappedPropertyGaps()
    {
      var preview = _sut.AnalyzeArmTemplate(SimpleTemplate);

        // tenantId and enableSoftDelete on KeyVault should be reported as unmapped
        preview.Gaps.Should().Contain(g =>
            g.Category == ImportPreviewGapCategory.UnmappedProperty
            && g.SourceResourceName == "myKeyVault"
            && g.Message.Contains("tenantId"));
    }

    // ── Template 2: Dependencies — ASP + WebApp + LAW + AppInsights ───

    private const string DependencyTemplate = """
    {
      "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
      "contentVersion": "1.0.0.0",
      "resources": [
        {
          "type": "Microsoft.Web/serverfarms",
          "apiVersion": "2023-12-01",
          "name": "myAppServicePlan",
          "location": "[resourceGroup().location]",
          "sku": { "name": "B1", "tier": "Basic" },
          "properties": {
            "reserved": true
          }
        },
        {
          "type": "Microsoft.Web/sites",
          "apiVersion": "2023-12-01",
          "name": "myWebApp",
          "location": "[resourceGroup().location]",
          "dependsOn": [
            "[resourceId('Microsoft.Web/serverfarms', 'myAppServicePlan')]"
          ],
          "properties": {
            "serverFarmId": "[resourceId('Microsoft.Web/serverfarms', 'myAppServicePlan')]",
            "httpsOnly": true,
            "siteConfig": {
              "linuxFxVersion": "DOTNETCORE|8.0",
              "alwaysOn": true
            }
          }
        },
        {
          "type": "Microsoft.OperationalInsights/workspaces",
          "apiVersion": "2023-09-01",
          "name": "myLogAnalytics",
          "location": "[resourceGroup().location]",
          "properties": {
            "retentionInDays": 30,
            "sku": { "name": "PerGB2018" }
          }
        },
        {
          "type": "Microsoft.Insights/components",
          "apiVersion": "2020-02-02",
          "name": "myAppInsights",
          "location": "[resourceGroup().location]",
          "kind": "web",
          "dependsOn": [
            "[resourceId('Microsoft.OperationalInsights/workspaces', 'myLogAnalytics')]"
          ],
          "properties": {
            "Application_Type": "web",
            "WorkspaceResourceId": "[resourceId('Microsoft.OperationalInsights/workspaces', 'myLogAnalytics')]"
          }
        }
      ]
    }
    """;

    [Fact]
    public void DependencyTemplate_ParsesAllFourResources()
    {
      var preview = _sut.AnalyzeArmTemplate(DependencyTemplate);

      preview.Resources.Should().HaveCount(4);
        preview.UnsupportedResources.Should().BeEmpty();
    }

    [Fact]
    public void DependencyTemplate_MapsDependencies()
    {
      var preview = _sut.AnalyzeArmTemplate(DependencyTemplate);

      var deps = preview.Dependencies;
        deps.Should().Contain(d =>
            d.FromResourceName == "myWebApp"
            && d.DependencyType == "dependsOn");
        deps.Should().Contain(d =>
            d.FromResourceName == "myAppInsights"
            && d.DependencyType == "dependsOn");
    }

    [Fact]
    public void DependencyTemplate_MapsAllResourceTypes()
    {
      var preview = _sut.AnalyzeArmTemplate(DependencyTemplate);

      var types = preview.Resources
            .Select(r => r.MappedResourceType)
            .ToList();

        types.Should().Contain(AzureResourceTypes.AppServicePlan);
        types.Should().Contain(AzureResourceTypes.WebApp);
        types.Should().Contain(AzureResourceTypes.LogAnalyticsWorkspace);
        types.Should().Contain(AzureResourceTypes.ApplicationInsights);
    }

    // ── Template 3: Mixed — supported + unsupported resources ─────────

    private const string MixedTemplate = """
    {
      "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
      "contentVersion": "1.0.0.0",
      "resources": [
        {
          "type": "Microsoft.KeyVault/vaults",
          "apiVersion": "2023-07-01",
          "name": "prodKeyVault",
          "location": "westeurope",
          "properties": {
            "sku": { "family": "A", "name": "premium" },
            "tenantId": "00000000-0000-0000-0000-000000000000"
          }
        },
        {
          "type": "Microsoft.Sql/servers",
          "apiVersion": "2023-08-01-preview",
          "name": "prodSqlServer",
          "location": "westeurope",
          "properties": {
            "administratorLogin": "sqladmin",
            "version": "12.0"
          }
        },
        {
          "type": "Microsoft.Sql/servers/databases",
          "apiVersion": "2023-08-01-preview",
          "name": "prodSqlServer/prodDb",
          "location": "westeurope",
          "dependsOn": [
            "[resourceId('Microsoft.Sql/servers', 'prodSqlServer')]"
          ],
          "sku": { "name": "Basic", "tier": "Basic" },
          "properties": {}
        },
        {
          "type": "Microsoft.Network/virtualNetworks",
          "apiVersion": "2024-01-01",
          "name": "prodVnet",
          "location": "westeurope",
          "properties": {
            "addressSpace": { "addressPrefixes": ["10.0.0.0/16"] }
          }
        },
        {
          "type": "Microsoft.Network/networkSecurityGroups",
          "apiVersion": "2024-01-01",
          "name": "prodNsg",
          "location": "westeurope",
          "properties": {}
        },
        {
          "type": "Microsoft.ServiceBus/namespaces",
          "apiVersion": "2024-01-01",
          "name": "prodServiceBus",
          "location": "westeurope",
          "sku": { "name": "Standard", "tier": "Standard" },
          "properties": {}
        }
      ]
    }
    """;

    [Fact]
    public void MixedTemplate_ParsesAllSixResources()
    {
      var preview = _sut.AnalyzeArmTemplate(MixedTemplate);

      preview.Resources.Should().HaveCount(6);
    }

    [Fact]
    public void MixedTemplate_IdentifiesUnsupportedResources()
    {
      var preview = _sut.AnalyzeArmTemplate(MixedTemplate);

        // VNet and NSG are not supported by IFS
        preview.UnsupportedResources.Should().HaveCount(2);
        preview.UnsupportedResources.Should().Contain("prodVnet");
        preview.UnsupportedResources.Should().Contain("prodNsg");
    }

    [Fact]
    public void MixedTemplate_MapsSupportedResourcesCorrectly()
    {
      var preview = _sut.AnalyzeArmTemplate(MixedTemplate);

      var mapped = preview.Resources
            .Where(r => r.MappedResourceType is not null)
            .Select(r => r.MappedResourceType!)
            .ToList();

        mapped.Should().HaveCount(4);
        mapped.Should().Contain(AzureResourceTypes.KeyVault);
        mapped.Should().Contain(AzureResourceTypes.SqlServer);
        mapped.Should().Contain(AzureResourceTypes.SqlDatabase);
        mapped.Should().Contain(AzureResourceTypes.ServiceBusNamespace);
    }

    [Fact]
    public void MixedTemplate_GeneratesUnsupportedResourceGaps()
    {
      var preview = _sut.AnalyzeArmTemplate(MixedTemplate);

        preview.Gaps.Should().Contain(g =>
            g.Category == "unsupported_resource"
        && g.Severity == ImportPreviewGapSeverity.Warning
            && g.Message.Contains("Microsoft.Network/virtualNetworks"));

        preview.Gaps.Should().Contain(g =>
            g.Category == "unsupported_resource"
            && g.Message.Contains("Microsoft.Network/networkSecurityGroups"));
    }

    [Fact]
    public void MixedTemplate_ExtractsKeyVaultPremiumSku()
    {
      var preview = _sut.AnalyzeArmTemplate(MixedTemplate);

      var kv = preview.Resources
            .Single(r => r.MappedResourceType == AzureResourceTypes.KeyVault);

        kv.ExtractedProperties.Should().ContainKey("skuName")
            .WhoseValue.Should().Be("premium");
    }

    [Fact]
    public void MixedTemplate_CapturesSqlDependency()
    {
      var preview = _sut.AnalyzeArmTemplate(MixedTemplate);

      preview.Dependencies.Should().Contain(d =>
            d.FromResourceName == "prodSqlServer/prodDb"
            && d.DependencyType == "dependsOn");
    }

    // ── Shared analyzer summary ────────────────────────────────────────

    [Fact]
    public void SimpleTemplate_ReturnsCorrectSummary()
    {
      var preview = _sut.AnalyzeArmTemplate(SimpleTemplate);

      preview.SourceFormat.Should().Be(IacSourceFormat.ArmJson);
      preview.Resources.Should().HaveCount(2);
      preview.Summary.Should().Be("Parsed 2 resource(s): 2 mapped, 0 unsupported.");
    }
}
