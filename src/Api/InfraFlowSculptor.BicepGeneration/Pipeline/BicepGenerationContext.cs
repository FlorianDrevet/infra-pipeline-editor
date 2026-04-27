using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Pipeline;

/// <summary>
/// Mutable in-flight state passed between Bicep generation pipeline stages.
/// Created once per <see cref="BicepGenerationEngine.Generate"/> call.
/// Not thread-safe — each generation request must use its own instance.
/// </summary>
public sealed class BicepGenerationContext
{
    /// <summary>The original generation request.</summary>
    public required GenerationRequest Request { get; init; }

    /// <summary>Modules currently being built by the pipeline. Stages mutate this list in place.</summary>
    public List<ModuleWorkItem> WorkItems { get; } = [];

    /// <summary>Per-resource identity analysis output, populated by <c>IdentityAnalysisStage</c>.</summary>
    public IdentityAnalysisResult Identity { get; set; } = IdentityAnalysisResult.Empty;

    /// <summary>App-settings analysis output, populated by <c>AppSettingsAnalysisStage</c>.</summary>
    public AppSettingsAnalysisResult AppSettings { get; set; } = AppSettingsAnalysisResult.Empty;

    /// <summary>Resource ID → (Name, ResourceTypeName) lookup, populated by <c>ModuleBuildStage</c>.</summary>
    public Dictionary<Guid, (string Name, string ResourceTypeName)> ResourceIdToInfo { get; set; } = [];

    /// <summary>Final assembly output, populated by <c>AssemblyStage</c>.</summary>
    public GenerationResult? Result { get; set; }

    /// <summary>
    /// When <c>true</c>, the <see cref="Stages.IrOutputPruningStage"/> is a no-op for this run.
    /// Used by <see cref="BicepGenerationEngine.GenerateMonoRepo"/> to defer pruning until
    /// after mono-repo assembly so that pruning operates on the union of demands across
    /// every per-configuration <c>main.bicep</c>.
    /// </summary>
    public bool SkipOutputPruning { get; set; }
}

/// <summary>
/// Per-resource module being assembled. The <see cref="Module"/> reference is replaced
/// (via record <c>with</c>) by mutation stages.
/// </summary>
public sealed class ModuleWorkItem
{
    /// <summary>The source resource definition this module was generated from.</summary>
    public required ResourceDefinition Resource { get; init; }

    /// <summary>Current module record. Mutated in place by stages via record <c>with</c>.</summary>
    public required GeneratedTypeModule Module { get; set; }

    /// <summary>
    /// Structured IR specification for this module. All generators implement
    /// <see cref="Generators.IResourceTypeBicepSpecGenerator"/> — pipeline stages apply
    /// IR transformers exclusively.
    /// </summary>
    public required BicepModuleSpec Spec { get; set; }

    /// <summary>Identity kind targeted for this resource (<c>SystemAssigned</c>, <c>UserAssigned</c>,
    /// <c>SystemAssigned, UserAssigned</c>, or <c>null</c> when no identity is required).</summary>
    public string? IdentityKind { get; set; }

    /// <summary>True when this resource shares an ARM type with peers requiring different identity kinds,
    /// triggering parameterized identity injection rather than a hardcoded block.</summary>
    public bool UsesParameterizedIdentity { get; set; }
}

/// <summary>
/// Result of identity analysis: which resources require system / user identities and which ARM types
/// have mixed identity kinds across their instances.
/// </summary>
public sealed record IdentityAnalysisResult(
    HashSet<(string Name, string Type)> SystemIdentityResources,
    Dictionary<(string Name, string Type), List<string>> UserIdentityResources,
    HashSet<string> MixedIdentityArmTypes)
{
    /// <summary>Empty/default analysis result used as the initial context state.</summary>
    public static readonly IdentityAnalysisResult Empty = new(
        new HashSet<(string, string)>(),
        new Dictionary<(string, string), List<string>>(),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase));
}

/// <summary>
/// Result of app-settings analysis: outputs to inject per source resource and the set of compute
/// ARM types that host at least one resource with app settings.
/// </summary>
public sealed record AppSettingsAnalysisResult(
    Dictionary<string, List<(string OutputName, string BicepExpression, bool IsSecure)>> OutputsBySourceResource,
    HashSet<string> ComputeArmTypesWithAppSettings)
{
    /// <summary>Empty/default analysis result used as the initial context state.</summary>
    public static readonly AppSettingsAnalysisResult Empty = new(
        new Dictionary<string, List<(string, string, bool)>>(StringComparer.OrdinalIgnoreCase),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase));
}
