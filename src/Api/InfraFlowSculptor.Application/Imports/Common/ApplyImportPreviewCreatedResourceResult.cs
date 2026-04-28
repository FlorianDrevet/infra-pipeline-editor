namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Represents a resource created from the import preview.
/// </summary>
/// <param name="ResourceType">The created resource type.</param>
/// <param name="ResourceId">The created resource identifier.</param>
/// <param name="Name">The created resource name.</param>
public sealed record ApplyImportPreviewCreatedResourceResult(
    string ResourceType,
    string ResourceId,
    string Name);
