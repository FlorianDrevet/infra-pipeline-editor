namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Defines which kind of bootstrap pipeline the engine should emit.
/// </summary>
public enum BootstrapMode
{
    /// <summary>
    /// Default mode. The bootstrap is the unique owner of the project-level Azure DevOps
    /// shared resources: it creates pipeline definitions, environments, and variable groups.
    /// Used in <c>AllInOne</c> layout and for the infra side of <c>SplitInfraCode</c>.
    /// </summary>
    FullOwner = 0,

    /// <summary>
    /// Application-side bootstrap of a <c>SplitInfraCode</c> project. The bootstrap only creates
    /// application pipeline definitions and validates that the shared environments and variable
    /// groups (owned by the infra bootstrap) are already provisioned. It never creates them.
    /// </summary>
    ApplicationOnly = 1,
}
