namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Describes a single Azure DevOps environment to be created by the bootstrap pipeline.
/// </summary>
/// <param name="Name">The Azure DevOps environment name referenced by generated deployment jobs.</param>
/// <param name="DisplayName">The human-readable project environment name stored in InfraFlowSculptor.</param>
/// <param name="RequiresApproval">Whether InfraFlowSculptor marks this environment as requiring approval before deployment.</param>
public sealed record BootstrapEnvironmentDefinition(
    string Name,
    string DisplayName,
    bool RequiresApproval);