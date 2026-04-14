namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Result of mono-repo Bicep generation at project level.</summary>
/// <param name="CommonFileUris">Shared files under the Common/ directory (types.bicep, functions.bicep, modules/...).</param>
/// <param name="ConfigFileUris">Per-configuration files keyed by config name, then relative path.</param>
public record GenerateProjectBicepResponse(
    IReadOnlyDictionary<string, Uri> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris);
