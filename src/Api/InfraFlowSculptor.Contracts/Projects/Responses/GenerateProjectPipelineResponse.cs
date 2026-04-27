namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Result of mono-repo pipeline generation at project level.</summary>
/// <param name="CommonFileUris">Union of infra and app shared templates (backward compatibility).</param>
/// <param name="ConfigFileUris">Union of infra and app per-config files (backward compatibility).</param>
/// <param name="InfraCommonFileUris">Shared template files routed to the infrastructure repository.</param>
/// <param name="AppCommonFileUris">Shared application pipeline templates routed to the application-code repository.</param>
/// <param name="InfraConfigFileUris">Per-configuration files routed to the infrastructure repository.</param>
/// <param name="AppConfigFileUris">Per-configuration files (per-resource wrappers under <c>apps/</c>) routed to the application-code repository.</param>
public record GenerateProjectPipelineResponse(
    IReadOnlyDictionary<string, Uri> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris,
    IReadOnlyDictionary<string, Uri> InfraCommonFileUris,
    IReadOnlyDictionary<string, Uri> AppCommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> InfraConfigFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> AppConfigFileUris);
