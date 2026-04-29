namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Represents a resource skipped during the import apply flow.
/// </summary>
/// <param name="ResourceType">The skipped resource type.</param>
/// <param name="Name">The skipped resource name.</param>
/// <param name="Reason">The reason why automatic creation was skipped.</param>
public sealed record ApplyImportPreviewSkippedResourceResult(
    string ResourceType,
    string Name,
    string Reason);
