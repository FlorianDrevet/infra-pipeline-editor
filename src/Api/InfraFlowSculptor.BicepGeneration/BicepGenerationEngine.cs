using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.TextManipulation;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration;

/// <summary>
/// High-level facade for Bicep generation. Coordinates the staged
/// <see cref="BicepGenerationPipeline"/> and the post-assembly output pruning for both
/// single-configuration and mono-repo scenarios.
/// </summary>
/// <remarks>
/// The public surface (<see cref="Generate"/> and <see cref="GenerateMonoRepo"/>) is preserved
/// from the legacy implementation so that existing application-layer handlers keep working
/// without changes.
/// </remarks>
public sealed class BicepGenerationEngine
{
    private readonly BicepGenerationPipeline _pipeline;

    /// <summary>
    /// Creates the engine with the pipeline that drives staged generation. Stages are registered
    /// individually in DI and ordered by <see cref="IBicepGenerationStage.Order"/>.
    /// </summary>
    public BicepGenerationEngine(BicepGenerationPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    /// Generates the Bicep files for a single infrastructure configuration. Unused module
    /// outputs are pruned based on references found in <c>main.bicep</c>.
    /// </summary>
    public GenerationResult Generate(GenerationRequest request)
    {
        var context = RunPipeline(request);
        var result = context.Result
            ?? throw new InvalidOperationException("Pipeline assembly stage did not produce a generation result.");

        BicepOutputPruner.PruneSingleConfig(result);
        return result;
    }

    /// <summary>
    /// Generates Bicep files for an entire project in mono-repo mode. Each configuration is
    /// generated independently, then assembled into a shared common folder and per-configuration
    /// folders. Unused outputs in shared modules are pruned using the union of references from
    /// every per-configuration <c>main.bicep</c>.
    /// </summary>
    public MonoRepoGenerationResult GenerateMonoRepo(MonoRepoGenerationRequest request)
    {
        var perConfigResults = new Dictionary<string, GenerationResult>();
        var hasAnyRoleAssignments = false;

        foreach (var (configName, configRequest) in request.ConfigRequests)
        {
            var context = RunPipeline(configRequest);
            perConfigResults[configName] = context.Result
                ?? throw new InvalidOperationException(
                    $"Pipeline assembly stage did not produce a generation result for configuration '{configName}'.");

            if (configRequest.RoleAssignments.Count > 0)
                hasAnyRoleAssignments = true;
        }

        var monoResult = MonoRepoBicepAssembler.Assemble(
            perConfigResults,
            request.NamingContext,
            request.Environments,
            hasAnyRoleAssignments,
            request.FlattenShared);

        BicepOutputPruner.PruneMonoRepo(monoResult, perConfigResults);

        return monoResult;
    }

    private BicepGenerationContext RunPipeline(GenerationRequest request)
    {
        var context = new BicepGenerationContext { Request = request };
        _pipeline.Execute(context);
        return context;
    }
}
