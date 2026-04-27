namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Describes a single variable inside a <see cref="BootstrapVariableGroupDefinition"/>.
/// </summary>
/// <param name="Name">The variable key name.</param>
/// <param name="Value">The plain-text value. Pass an empty string for secret variables — the user fills them manually.</param>
/// <param name="IsSecret">When <c>true</c>, the variable is marked as secret in Azure DevOps (value will be empty).</param>
public sealed record BootstrapVariable(
    string Name,
    string Value,
    bool IsSecret);

/// <summary>
/// Describes an Azure DevOps variable group to be created by the bootstrap pipeline.
/// </summary>
/// <param name="GroupName">The display name of the variable group.</param>
/// <param name="Variables">The list of variables with their values and secret flags.</param>
public sealed record BootstrapVariableGroupDefinition(
    string GroupName,
    IReadOnlyList<BootstrapVariable> Variables);
