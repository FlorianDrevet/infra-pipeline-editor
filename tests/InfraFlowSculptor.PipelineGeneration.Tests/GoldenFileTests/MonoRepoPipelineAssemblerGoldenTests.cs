using InfraFlowSculptor.PipelineGeneration.Models;
using InfraFlowSculptor.PipelineGeneration.Tests.Common;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests.GoldenFileTests;

/// <summary>
/// Byte-for-byte parity tests for <see cref="MonoRepoPipelineAssembler"/>.
/// Goldens are captured under <c>GoldenFiles/MonoRepo/</c> via <c>IFS_UPDATE_GOLDEN=true</c>.
/// </summary>
public sealed class MonoRepoPipelineAssemblerGoldenTests
{
    private readonly PipelineGenerationEngine _engine = new();

    [Fact]
    public void Given_TwoConfigsThreeEnvs_When_Assemble_Then_CommonFilesMatchGolden()
    {
        // Arrange
        var result = AssembleTwoConfigs();

        // Act & Assert
        GoldenFileAssertion.AssertDictionaryMatches(result.CommonFiles, "MonoRepo/two-configs/CommonFiles");
    }

    [Fact]
    public void Given_TwoConfigsThreeEnvs_When_Assemble_Then_ConfigFilesMatchGolden()
    {
        // Arrange
        var result = AssembleTwoConfigs();

        // Act
        var flattened = new Dictionary<string, string>();
        foreach (var (configName, files) in result.ConfigFiles)
        {
            foreach (var (filePath, content) in files)
            {
                flattened[$"{configName}/{filePath}"] = content;
            }
        }

        // Assert
        GoldenFileAssertion.AssertDictionaryMatches(flattened, "MonoRepo/two-configs/ConfigFiles");
    }

    private MonoRepoPipelineResult AssembleTwoConfigs()
    {
        var coreRequest = GenerationRequestFixtures.StandardStandalone();
        var sharedRequest = GenerationRequestFixtures.WithVariableGroupsAndSecureParams();

        var perConfigResults = new Dictionary<string, PipelineGenerationResult>
        {
            ["core"] = _engine.Generate(coreRequest, "core", isMonoRepo: true),
            ["shared"] = _engine.Generate(sharedRequest, "shared", isMonoRepo: true),
        };

        var environments = GenerationRequestFixtures.ThreeEnvironments();

        return MonoRepoPipelineAssembler.Assemble(perConfigResults, environments);
    }
}
