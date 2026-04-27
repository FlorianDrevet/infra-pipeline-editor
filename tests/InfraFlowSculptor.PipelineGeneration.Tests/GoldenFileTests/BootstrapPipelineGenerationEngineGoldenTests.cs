using InfraFlowSculptor.PipelineGeneration.Models;
using InfraFlowSculptor.PipelineGeneration.Tests.Common;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests.GoldenFileTests;

/// <summary>
/// Byte-for-byte parity tests for <see cref="BootstrapPipelineGenerationEngine"/>.
/// Goldens are captured under <c>GoldenFiles/Bootstrap/</c> via <c>IFS_UPDATE_GOLDEN=true</c>.
/// </summary>
public sealed class BootstrapPipelineGenerationEngineGoldenTests
{
    private readonly BootstrapPipelineGenerationEngine _sut = new();

    [Fact]
    public void Given_FullOwnerComplete_When_Generate_Then_OutputMatchesGolden()
    {
        // Arrange
        var request = BootstrapRequestFixtures.FullOwnerComplete();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Bootstrap/full-owner-complete");
    }

    [Fact]
    public void Given_FullOwnerPipelinesOnly_When_Generate_Then_OutputMatchesGolden()
    {
        // Arrange
        var request = BootstrapRequestFixtures.FullOwnerPipelinesOnly();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Bootstrap/full-owner-pipelines-only");
    }

    [Fact]
    public void Given_ApplicationOnlyComplete_When_Generate_Then_OutputMatchesGolden()
    {
        // Arrange
        var request = BootstrapRequestFixtures.ApplicationOnlyComplete();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Bootstrap/application-only-complete");
    }

    [Fact]
    public void Given_ApplicationOnlyPipelinesOnly_When_Generate_Then_OutputMatchesGolden()
    {
        // Arrange
        var request = BootstrapRequestFixtures.ApplicationOnlyPipelinesOnly();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Bootstrap/application-only-pipelines-only");
    }

    [Fact]
    public void Given_EmptyRequest_When_Generate_Then_OutputMatchesGoldenNoOp()
    {
        // Arrange
        var request = BootstrapRequestFixtures.Empty();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Bootstrap/empty-noop");
    }
}
