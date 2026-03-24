namespace InfraFlowSculptor.Application.AppSettings.Queries.GetAvailableOutputs;

/// <summary>Result containing the available outputs for a resource type.</summary>
public sealed record AvailableOutputsResult(
    string ResourceTypeName,
    IReadOnlyList<OutputDefinitionResult> Outputs);

/// <summary>Describes a single output.</summary>
public sealed record OutputDefinitionResult(
    string Name,
    string Description);
