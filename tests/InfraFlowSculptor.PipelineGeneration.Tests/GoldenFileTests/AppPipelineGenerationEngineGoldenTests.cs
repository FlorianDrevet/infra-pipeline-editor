using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Common;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests.GoldenFileTests;

/// <summary>
/// Byte-for-byte parity tests for <see cref="AppPipelineGenerationEngine"/>.
/// Goldens are captured under <c>GoldenFiles/AppPipeline/</c> via <c>IFS_UPDATE_GOLDEN=true</c>.
/// </summary>
public sealed class AppPipelineGenerationEngineGoldenTests
{
    private readonly AppPipelineGenerationEngine _sut = new(new IAppPipelineGenerator[]
    {
        new ContainerAppPipelineGenerator(),
        new WebAppCodePipelineGenerator(),
        new WebAppContainerPipelineGenerator(),
        new FunctionAppCodePipelineGenerator(),
        new FunctionAppContainerPipelineGenerator(),
    });

    [Fact]
    public void Given_ContainerApp_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.ContainerApp();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.Files, "AppPipeline/container-app");
    }

    [Fact]
    public void Given_WebAppCode_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.WebAppCode();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.Files, "AppPipeline/web-app-code");
    }

    [Fact]
    public void Given_WebAppContainerManagedIdentity_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.WebAppContainer_ManagedIdentity();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.Files, "AppPipeline/web-app-container-mi");
    }

    [Fact]
    public void Given_WebAppContainerAdminCredentials_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.WebAppContainer_AdminCredentials();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.Files, "AppPipeline/web-app-container-admin");
    }

    [Fact]
    public void Given_FunctionAppCode_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.FunctionAppCode();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.Files, "AppPipeline/function-app-code");
    }

    [Fact]
    public void Given_FunctionAppContainer_When_Generate_Then_AllFilesMatchGolden()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.FunctionAppContainer();

        // Act
        var result = _sut.Generate(request);

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.Files, "AppPipeline/function-app-container");
    }

    [Fact]
    public void Given_TwoComputeResources_When_GenerateAllCombined_Then_AllFilesMatchGolden()
    {
        // Arrange
        var requests = AppPipelineRequestFixtures.TwoComputeResourcesForCombined();

        // Act
        var result = _sut.GenerateAll(requests, AppPipelineMode.Combined, "core");

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.Files, "AppPipeline/combined-two-resources");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_AllFilesMatchGolden()
    {
        // Arrange & Act
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(files, "AppPipeline/shared-templates");
    }
}
