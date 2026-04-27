using InfraFlowSculptor.PipelineGeneration.Tests.Common;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests.GoldenFileTests;

/// <summary>
/// Byte-for-byte parity tests for <see cref="PipelineGenerationEngine"/>.
/// Goldens are captured under <c>GoldenFiles/Engine/</c> via <c>IFS_UPDATE_GOLDEN=true</c>.
/// </summary>
public sealed class PipelineGenerationEngineGoldenTests
{
    private readonly PipelineGenerationEngine _sut = new();

    [Fact]
    public void Given_StandaloneRequest_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = GenerationRequestFixtures.StandardStandalone();

        // Act
        var result = _sut.Generate(request, "core", isMonoRepo: false);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Engine/standalone-default");
    }

    [Fact]
    public void Given_MonoRepoRequest_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = GenerationRequestFixtures.StandardStandalone();

        // Act
        var result = _sut.Generate(request, "core", isMonoRepo: true);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Engine/monorepo-default");
    }

    [Fact]
    public void Given_RequestWithVariableGroupsAndSecureParams_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = GenerationRequestFixtures.WithVariableGroupsAndSecureParams();

        // Act
        var result = _sut.Generate(request, "core", isMonoRepo: false);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.TemplateFiles, "Engine/standalone-with-vargroups");
    }

    [Fact]
    public void Given_SharedTemplatesOneEnv_When_GenerateSharedTemplates_Then_AllFilesMatchGolden()
    {
        // Arrange
        var environments = GenerationRequestFixtures.OneEnvironment();

        // Act
        var files = PipelineGenerationEngine.GenerateSharedTemplates(["core"], environments);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(files, "Engine/shared-templates-1env");
    }

    [Fact]
    public void Given_SharedTemplatesThreeEnvs_When_GenerateSharedTemplates_Then_AllFilesMatchGolden()
    {
        // Arrange
        var environments = GenerationRequestFixtures.ThreeEnvironments();

        // Act
        var files = PipelineGenerationEngine.GenerateSharedTemplates(["core"], environments);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(files, "Engine/shared-templates-3envs");
    }
}
