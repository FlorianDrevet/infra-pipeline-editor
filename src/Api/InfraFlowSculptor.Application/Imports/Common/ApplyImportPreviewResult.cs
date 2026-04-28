namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Represents the outcome of applying an import preview to a new project.
/// </summary>
public sealed record ApplyImportPreviewResult
{
    /// <summary>
    /// Gets the operation status.
    /// </summary>
    public string Status { get; init; } = "applied";

    /// <summary>
    /// Gets the created project identifier.
    /// </summary>
    public required string ProjectId { get; init; }

    /// <summary>
    /// Gets the created project name.
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    /// Gets the created infrastructure configuration identifier when infrastructure creation succeeded.
    /// </summary>
    public string? InfrastructureConfigId { get; init; }

    /// <summary>
    /// Gets the created default resource group identifier when infrastructure creation succeeded.
    /// </summary>
    public string? ResourceGroupId { get; init; }

    /// <summary>
    /// Gets the infrastructure creation error when project creation succeeded but infrastructure creation failed.
    /// </summary>
    public string? InfrastructureError { get; init; }

    /// <summary>
    /// Gets the resources created from the import preview.
    /// </summary>
    public IReadOnlyList<ApplyImportPreviewCreatedResourceResult> CreatedResources { get; init; } = [];

    /// <summary>
    /// Gets the resources skipped during automatic creation.
    /// </summary>
    public IReadOnlyList<ApplyImportPreviewSkippedResourceResult> SkippedResources { get; init; } = [];

    /// <summary>
    /// Gets the suggested next steps after the apply operation.
    /// </summary>
    public IReadOnlyList<string> NextSuggestedActions { get; init; } = [];
}

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