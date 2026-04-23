using InfraFlowSculptor.BicepGeneration;
using Xunit;

namespace InfraFlowSculptor.GenerationParity.Tests;

/// <summary>
/// Parity tests for <see cref="BicepGenerationEngine"/>. These are byte-for-byte
/// golden-file assertions intended as a safety net during V2 multi-repo topology
/// refactors (steps A2–A3–D1). A failure here means engine output has drifted —
/// verify the drift is intentional and regenerate goldens explicitly.
/// </summary>
public sealed class BicepParityTests
{
    [Fact]
    public void SingleConfig_MatchesGolden()
    {
        var engine = FixtureBuilders.CreateBicepEngine();
        var request = FixtureBuilders.BuildSingleConfigRequest();

        var result = engine.Generate(request);

        GoldenComparer.AssertMatchesGolden("bicep-single-config", result.Files);
    }

    [Fact]
    public void MonoRepoTwoConfigs_MatchesGolden()
    {
        var engine = FixtureBuilders.CreateBicepEngine();
        var request = FixtureBuilders.BuildMonoRepoRequest();

        var result = engine.GenerateMonoRepo(request);

        var flat = GoldenComparer.FlattenMonoRepo(result.CommonFiles, result.ConfigFiles);
        GoldenComparer.AssertMatchesGolden("bicep-mono-repo", flat);
    }
}
