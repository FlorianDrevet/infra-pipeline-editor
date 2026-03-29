namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Result of mono-repo pipeline generation at project level.</summary>
/// <param name="ConfigFileUris">Per-configuration files keyed by config name, then relative path.</param>
public record GenerateProjectPipelineResponse(
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris);
