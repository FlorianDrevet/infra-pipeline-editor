namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Defines how container images are promoted between environments.
/// </summary>
public enum AppPipelinePromotionStrategy
{
    /// <summary>
    /// Reuses a shared registry and only creates or updates environment tags.
    /// </summary>
    TagOnly = 0,

    /// <summary>
    /// Imports the immutable image from the build registry into the target environment registry.
    /// </summary>
    AcrImport = 1,
}