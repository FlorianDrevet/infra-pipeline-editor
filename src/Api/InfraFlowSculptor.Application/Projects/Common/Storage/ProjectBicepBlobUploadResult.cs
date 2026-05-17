namespace InfraFlowSculptor.Application.Projects.Common.Storage;

/// <summary>
/// Contains the blob URIs produced by a mono-repo Bicep upload batch.
/// </summary>
/// <param name="CommonFileUris">Shared files uploaded under the <c>Common/</c> folder.</param>
/// <param name="ConfigFileUris">Per-configuration files keyed by configuration name and relative path.</param>
public sealed record ProjectBicepBlobUploadResult(
    IReadOnlyDictionary<string, Uri> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris);
