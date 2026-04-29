using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Mcp.Imports;
using InfraFlowSculptor.Mcp.Imports.Models;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Imports;

public sealed class ImportPreviewServiceTests
{
  private const string SourceContent = "arm content";

  private readonly IImportPreviewAnalyzer _analyzer;
  private readonly ImportPreviewService _sut;

  public ImportPreviewServiceTests()
  {
    _analyzer = Substitute.For<IImportPreviewAnalyzer>();
    _sut = new ImportPreviewService(_analyzer);
  }

    [Fact]
  public void Given_AnalyzerResult_When_CreatePreviewFromArm_Then_StoresPreviewWithGeneratedId()
    {
    // Arrange
    var analysis = CreateAnalysisResult();
    _analyzer.AnalyzeArmTemplate(SourceContent).Returns(analysis);

        // Act
    var preview = _sut.CreatePreviewFromArm(SourceContent);

        // Assert
        preview.PreviewId.Should().StartWith("preview_");
        preview.Analysis.SourceFormat.Should().Be(IacSourceFormat.ArmJson);
        preview.Analysis.Resources.Should().HaveCount(1);
      preview.Analysis.Dependencies.Should().ContainSingle();
      preview.Analysis.Metadata["schema"].Should().Be("https://example/schema");
      preview.Analysis.Gaps.Should().ContainSingle();
      preview.Analysis.UnsupportedResources.Should().ContainSingle().Which.Should().Be("legacyNetwork");

    var storedPreview = _sut.GetPreview(preview.PreviewId);
    storedPreview.Should().BeEquivalentTo(preview);

        var resource = preview.Analysis.Resources[0];
        resource.SourceType.Should().Be("Microsoft.KeyVault/vaults");
        resource.SourceName.Should().Be("myKeyVault");
        resource.MappedResourceType.Should().Be("KeyVault");
      resource.Confidence.Should().Be(ImportPreviewMappingConfidence.High);
    _analyzer.Received(1).AnalyzeArmTemplate(SourceContent);
    }

    [Fact]
  public void Given_MissingPreviewId_When_GetPreview_Then_ReturnsNull()
    {
        // Act
    var preview = _sut.GetPreview("preview_missing");

        // Assert
    preview.Should().BeNull();
    }

    [Fact]
  public void Given_StoredPreview_When_RemovePreview_Then_RemovesItFromMemory()
    {
    // Arrange
    _analyzer.AnalyzeArmTemplate(SourceContent).Returns(CreateAnalysisResult());
    var preview = _sut.CreatePreviewFromArm(SourceContent);

        // Act
    var removed = _sut.RemovePreview(preview.PreviewId);

        // Assert
    removed.Should().BeTrue();
    _sut.GetPreview(preview.PreviewId).Should().BeNull();
    }

    [Fact]
  public void Given_MissingPreviewId_When_RemovePreview_Then_ReturnsFalse()
    {
        // Act
    var removed = _sut.RemovePreview("preview_missing");

        // Assert
    removed.Should().BeFalse();
    }

  private static ImportPreviewAnalysisResult CreateAnalysisResult()
    {
    return new ImportPreviewAnalysisResult
    {
      SourceFormat = IacSourceFormat.ArmJson,
      Resources =
      [
        new ImportedResourceAnalysisResult
        {
          SourceType = "Microsoft.KeyVault/vaults",
          SourceName = "myKeyVault",
          MappedResourceType = "KeyVault",
          MappedName = "myKeyVault",
          Confidence = ImportPreviewMappingConfidence.High,
          ExtractedProperties = new Dictionary<string, object?>
          {
            ["skuName"] = "standard",
          },
        },
      ],
      Dependencies =
      [
        new ImportedDependencyAnalysisResult("myKeyVault", "sharedIdentity", ImportDependencyType.DependsOn),
      ],
      Metadata = new Dictionary<string, string>
      {
        ["schema"] = "https://example/schema",
      },
      Gaps =
      [
        new ImportPreviewGapResult
        {
          Severity = ImportPreviewGapSeverity.Warning,
          Category = ImportPreviewGapCategory.UnsupportedResource,
          Message = "Resource type 'Microsoft.Network/virtualNetworks' is not supported by InfraFlowSculptor.",
          SourceResourceName = "legacyNetwork",
        },
      ],
      UnsupportedResources = ["legacyNetwork"],
      Summary = "Parsed 1 resource(s): 1 mapped, 1 unsupported.",
    };
    }
}
