namespace InfraFlowSculptor.Application.Projects.Common.Storage;

/// <summary>
/// Contains the blob URIs produced by a mono-repo pipeline upload batch.
/// </summary>
/// <param name="CommonFileUris">Union of infrastructure and application shared template URIs.</param>
/// <param name="ConfigFileUris">Union of infrastructure and application per-configuration URIs.</param>
/// <param name="InfraCommonFileUris">Infrastructure shared template URIs.</param>
/// <param name="AppCommonFileUris">Application shared template URIs.</param>
/// <param name="InfraConfigFileUris">Infrastructure per-configuration URIs.</param>
/// <param name="AppConfigFileUris">Application per-configuration URIs.</param>
public sealed record ProjectPipelineBlobUploadResult(
    IReadOnlyDictionary<string, Uri> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris,
    IReadOnlyDictionary<string, Uri> InfraCommonFileUris,
    IReadOnlyDictionary<string, Uri> AppCommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> InfraConfigFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> AppConfigFileUris);