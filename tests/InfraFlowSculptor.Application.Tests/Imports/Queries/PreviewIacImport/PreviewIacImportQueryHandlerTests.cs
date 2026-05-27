using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Imports.Common.Analysis;
using InfraFlowSculptor.Application.Imports.Common.Constants;
using InfraFlowSculptor.Application.Imports.Queries.PreviewIacImport;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Imports.Queries.PreviewIacImport;

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
    public async Task Given_UnsupportedFormat_When_Handle_Then_ReturnsUnsupportedSummaryAsync()
    {
        // Arrange
        var query = new PreviewIacImportQuery("terraform-hcl", "resource {}");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.SourceFormat.Should().Be("terraform-hcl");
        result.Value.Summary.Should().Contain("not supported");
        _analyzer.DidNotReceive().AnalyzeArmTemplate(Arg.Any<string>());
    }

    [Fact]
    public async Task Given_ArmJsonFormat_When_Handle_Then_DelegatesToAnalyzerAsync()
    {
        // Arrange
        const string armContent = "{\"$schema\":\"https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#\"}";
        var expectedResult = new ImportPreviewAnalysisResult
        {
            SourceFormat = IacSourceFormat.ArmJson,
            Summary = "Analyzed 3 resources."
        };
        _analyzer.AnalyzeArmTemplate(armContent).Returns(expectedResult);
        var query = new PreviewIacImportQuery(IacSourceFormat.ArmJson, armContent);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedResult);
        _analyzer.Received(1).AnalyzeArmTemplate(armContent);
    }

    [Fact]
    public async Task Given_ArmJsonFormatCaseInsensitive_When_Handle_Then_DelegatesToAnalyzerAsync()
    {
        // Arrange
        const string armContent = "{}";
        var expectedResult = new ImportPreviewAnalysisResult
        {
            SourceFormat = "ARM-JSON",
            Summary = "Analyzed."
        };
        _analyzer.AnalyzeArmTemplate(armContent).Returns(expectedResult);
        var query = new PreviewIacImportQuery("ARM-JSON", armContent);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _analyzer.Received(1).AnalyzeArmTemplate(armContent);
    }
}
