using ErrorOr;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.GenerationCore.Errors;
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
    private const string MissingBicepGeneratorMessagePrefix = "No Bicep generator registered for resource '";
    private const string UnsupportedRoleAssignmentModuleMessagePrefix = "Role assignment module not supported for resource type '";
    private const string UnsupportedRoleAssignmentResourceTypeMessagePrefix = "Resource type '";
    private const string InvalidKeyVaultSecretNameMessagePrefix = "Key Vault secret name '";

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
    public ErrorOr<GenerationResult> Generate(
        GenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var context = RunPipeline(request, skipOutputPruning: false, cancellationToken);
            return context.Result
                ?? throw new InvalidOperationException("Pipeline assembly stage did not produce a generation result.");
        }
        catch (NotSupportedException exception) when (TryMapExpectedException(exception, out var error))
        {
            return error;
        }
        catch (InvalidOperationException exception) when (TryMapExpectedException(exception, out var error))
        {
            return error;
        }
    }

    /// <summary>
    /// Generates Bicep files for an entire project in mono-repo mode. Each configuration is
    /// generated independently with per-config pruning skipped, then assembled into a shared
    /// common folder and per-configuration folders. Unused outputs in shared modules are
    /// pruned using the union of references from every per-configuration <c>main.bicep</c>.
    /// </summary>
    public ErrorOr<MonoRepoGenerationResult> GenerateMonoRepo(
        MonoRepoGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var perConfigResults = new Dictionary<string, GenerationResult>();
            var perConfigContexts = new Dictionary<string, BicepGenerationContext>();
            var hasAnyRoleAssignments = false;

            foreach (var (configName, configRequest) in request.ConfigRequests)
            {
                var context = RunPipeline(configRequest, skipOutputPruning: true, cancellationToken);
                perConfigResults[configName] = context.Result
                    ?? throw new InvalidOperationException(
                        $"Pipeline assembly stage did not produce a generation result for configuration '{configName}'.");
                perConfigContexts[configName] = context;

                if (configRequest.RoleAssignments.Count > 0)
                    hasAnyRoleAssignments = true;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var monoResult = MonoRepoBicepAssembler.Assemble(
                perConfigResults,
                request.NamingContext,
                request.Environments,
                hasAnyRoleAssignments,
                request.FlattenShared);

            cancellationToken.ThrowIfCancellationRequested();
            IrMonoRepoOutputPruner.Prune(monoResult, perConfigResults, perConfigContexts);

            return monoResult;
        }
        catch (NotSupportedException exception) when (TryMapExpectedException(exception, out var error))
        {
            return error;
        }
        catch (InvalidOperationException exception) when (TryMapExpectedException(exception, out var error))
        {
            return error;
        }
    }

    private BicepGenerationContext RunPipeline(
        GenerationRequest request,
        bool skipOutputPruning,
        CancellationToken cancellationToken)
    {
        var context = new BicepGenerationContext
        {
            Request = request,
            CancellationToken = cancellationToken,
            SkipOutputPruning = skipOutputPruning,
        };
        _pipeline.Execute(context);
        return context;
    }

    private static bool TryMapExpectedException(Exception exception, out Error error)
    {
        switch (exception)
        {
            case NotSupportedException notSupportedException when notSupportedException.Message.StartsWith(MissingBicepGeneratorMessagePrefix, StringComparison.Ordinal):
                error = GenerationErrors.UnsupportedBicepResourceType(notSupportedException.Message);
                return true;

            case NotSupportedException notSupportedException when notSupportedException.Message.StartsWith(UnsupportedRoleAssignmentModuleMessagePrefix, StringComparison.Ordinal):
                error = GenerationErrors.UnsupportedBicepFeature(notSupportedException.Message);
                return true;

            case NotSupportedException notSupportedException when notSupportedException.Message.StartsWith(UnsupportedRoleAssignmentResourceTypeMessagePrefix, StringComparison.Ordinal):
                error = GenerationErrors.UnsupportedBicepFeature(notSupportedException.Message);
                return true;

            case InvalidOperationException invalidOperationException when invalidOperationException.Message.StartsWith(InvalidKeyVaultSecretNameMessagePrefix, StringComparison.Ordinal):
                error = GenerationErrors.InvalidBicepConfiguration(invalidOperationException.Message);
                return true;

            default:
                error = default;
                return false;
        }
    }
}
