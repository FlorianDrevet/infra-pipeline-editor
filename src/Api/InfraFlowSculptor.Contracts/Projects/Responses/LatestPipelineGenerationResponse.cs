namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Pipeline file paths from the latest generation.</summary>
/// <param name="CommonFileUris">Union of infra and app shared templates for backward compatibility.</param>
/// <param name="ConfigFileUris">Union of infra and app per-configuration files for backward compatibility.</param>
/// <param name="InfraCommonFileUris">Shared template files routed to the infrastructure repository.</param>
/// <param name="AppCommonFileUris">Shared template files routed to the application-code repository.</param>
/// <param name="InfraConfigFileUris">Per-configuration files routed to the infrastructure repository.</param>
/// <param name="AppConfigFileUris">Per-configuration files routed to the application-code repository.</param>
public record LatestPipelineGenerationResponse(
    IReadOnlyDictionary<string, string> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFileUris,
    IReadOnlyDictionary<string, string> InfraCommonFileUris,
    IReadOnlyDictionary<string, string> AppCommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> InfraConfigFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AppConfigFileUris);