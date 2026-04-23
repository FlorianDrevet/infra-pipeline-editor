using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using Xunit;

namespace InfraFlowSculptor.GenerationParity.Tests;

/// <summary>
/// Parity tests for <see cref="PipelineGenerationEngine"/> and
/// <see cref="MonoRepoPipelineAssembler"/>. Byte-for-byte golden assertions.
/// </summary>
public sealed class PipelineParityTests
{
    [Fact]
    public void SingleConfig_MatchesGolden()
    {
        var engine = FixtureBuilders.CreatePipelineEngine();
        var request = FixtureBuilders.BuildSingleConfigRequest();

        var result = engine.Generate(request, "default", isMonoRepo: false);

        GoldenComparer.AssertMatchesGolden("pipeline-single-config", result.Files);
    }

    [Fact]
    public void MonoRepoTwoConfigs_MatchesGolden()
    {
        var engine = FixtureBuilders.CreatePipelineEngine();
        var monoRequest = FixtureBuilders.BuildMonoRepoRequest();

        var perConfig = new Dictionary<string, PipelineGenerationResult>();
        foreach (var (configName, configRequest) in monoRequest.ConfigRequests)
        {
            perConfig[configName] = engine.Generate(configRequest, configName, isMonoRepo: true);
        }

        var assembled = MonoRepoPipelineAssembler.Assemble(
            perConfig,
            monoRequest.Environments);

        var flat = GoldenComparer.FlattenMonoRepo(assembled.CommonFiles, assembled.ConfigFiles);
        GoldenComparer.AssertMatchesGolden("pipeline-mono-repo", flat);
    }
}
