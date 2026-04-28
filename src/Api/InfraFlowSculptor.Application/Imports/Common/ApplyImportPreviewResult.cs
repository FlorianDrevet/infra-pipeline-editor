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