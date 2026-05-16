namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;

/// <summary>Pipeline file paths grouped by common and per-config, with infra/app split.</summary>
/// <param name="CommonFilePaths">Union of infra and app shared templates for backward compatibility.</param>
/// <param name="ConfigFilePaths">Union of infra and app per-configuration files for backward compatibility.</param>
/// <param name="InfraCommonFilePaths">Shared template files routed to the infrastructure repository.</param>
/// <param name="AppCommonFilePaths">Shared template files routed to the application-code repository.</param>
/// <param name="InfraConfigFilePaths">Per-configuration files routed to the infrastructure repository.</param>
/// <param name="AppConfigFilePaths">Per-configuration files routed to the application-code repository.</param>
public record LatestPipelineFiles(
    IReadOnlyDictionary<string, string> CommonFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFilePaths,
    IReadOnlyDictionary<string, string> InfraCommonFilePaths,
    IReadOnlyDictionary<string, string> AppCommonFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> InfraConfigFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AppConfigFilePaths);