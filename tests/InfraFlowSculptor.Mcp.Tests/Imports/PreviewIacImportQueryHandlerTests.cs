using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.Imports.Queries.PreviewIacImport;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Imports;

public sealed class PreviewIacImportQueryHandlerTests
{
    private readonly IImportPreviewAnalyzer _analyzer;
    private readonly PreviewIacImportQueryHandler _sut;

    public PreviewIacImportQueryHandlerTests()
    {
        _analyzer = Substitute.For<IImportPreviewAnalyzer>();
        _sut = new PreviewIacImportQueryHandler(_analyzer);
    }

    [Fact]
    public async Task Given_ArmJsonSourceFormat_When_Handle_Then_ReturnsAnalyzerResult_Async()
    {
        // Arrange
        var expected = new ImportPreviewAnalysisResult
        {
            SourceFormat = "arm-json",
            Resources =
            [
                new ImportedResourceAnalysisResult
                {
                    SourceType = "Microsoft.KeyVault/vaults",
                    SourceName = "myKeyVault",
                    MappedResourceType = "KeyVault",
                    MappedName = "myKeyVault",
                    Confidence = ImportPreviewMappingConfidence.High,
                },
            ],
            Summary = "Parsed 1 resource(s): 1 mapped, 0 unsupported.",
        };
        _analyzer.AnalyzeArmTemplate("arm content").Returns(expected);

        // Act
        var result = await _sut.Handle(
            new PreviewIacImportQuery("arm-json", "arm content"),
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeSameAs(expected);
        _analyzer.Received(1).AnalyzeArmTemplate("arm content");
    }

    [Fact]
    public async Task Given_UnsupportedSourceFormat_When_Handle_Then_ReturnsUnsupportedAnalysisWithoutCallingAnalyzer_Async()
    {
        // Act
        var result = await _sut.Handle(
            new PreviewIacImportQuery("terraform", "content"),
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.SourceFormat.Should().Be("terraform");
        result.Value.Resources.Should().BeEmpty();
        result.Value.Dependencies.Should().BeEmpty();
        result.Value.Gaps.Should().BeEmpty();
        result.Value.UnsupportedResources.Should().BeEmpty();
        result.Value.Summary.Should().Contain("not supported");
        _analyzer.DidNotReceiveWithAnyArgs().AnalyzeArmTemplate(default!);
    }
}