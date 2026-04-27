using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration;

/// <summary>
/// High-level facade for Bicep generation. Coordinates the staged
/// <see cref="BicepGenerationPipeline"/> for both single-configuration and mono-repo scenarios.
/// </summary>
/// <remarks>
/// The public surface (<see cref="Generate"/> and <see cref="GenerateMonoRepo"/>) is preserved
/// from the legacy implementation so that existing application-layer handlers keep working
/// without changes. Per-configuration output pruning is performed by the pipeline's
/// <see cref="Pipeline.Stages.IrOutputPruningStage"/>; mono-repo pruning is performed
/// by <see cref="IrMonoRepoOutputPruner"/> after shared assembly.
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
    /// outputs are pruned by the pipeline's <see cref="Pipeline.Stages.IrOutputPruningStage"/>.
    /// </summary>
    public GenerationResult Generate(GenerationRequest request)
    {
        var context = RunPipeline(request, skipOutputPruning: false);
        return context.Result
            ?? throw new InvalidOperationException("Pipeline assembly stage did not produce a generation result.");
    }

    /// <summary>
    /// Generates Bicep files for an entire project in mono-repo mode. Each configuration is
    /// generated independently with per-config pruning skipped, then assembled into a shared
    /// common folder and per-configuration folders. Unused outputs in shared modules are
    /// pruned using the union of references from every per-configuration <c>main.bicep</c>.
    /// </summary>
    public MonoRepoGenerationResult GenerateMonoRepo(MonoRepoGenerationRequest request)
    {
        var perConfigResults = new Dictionary<string, GenerationResult>();
        var perConfigContexts = new Dictionary<string, BicepGenerationContext>();
        var hasAnyRoleAssignments = false;

        foreach (var (configName, configRequest) in request.ConfigRequests)
        {
            var context = RunPipeline(configRequest, skipOutputPruning: true);
            perConfigResults[configName] = context.Result
                ?? throw new InvalidOperationException(
                    $"Pipeline assembly stage did not produce a generation result for configuration '{configName}'.");
            perConfigContexts[configName] = context;

            if (configRequest.RoleAssignments.Count > 0)
                hasAnyRoleAssignments = true;
        }

        var monoResult = MonoRepoBicepAssembler.Assemble(
            perConfigResults,
            request.NamingContext,
            request.Environments,
            hasAnyRoleAssignments,
            request.FlattenShared);

        IrMonoRepoOutputPruner.Prune(monoResult, perConfigResults, perConfigContexts);

        return monoResult;
    }

    private BicepGenerationContext RunPipeline(GenerationRequest request, bool skipOutputPruning)
    {
        var context = new BicepGenerationContext
        {
            Request = request,
            SkipOutputPruning = skipOutputPruning,
        };
        _pipeline.Execute(context);
        return context;
    }
}
