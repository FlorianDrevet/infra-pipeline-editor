namespace InfraFlowSculptor.Contracts.Imports.Responses;

/// <summary>
/// Response returned after applying an import preview.
/// </summary>
/// <param name="Status">The operation status.</param>
/// <param name="ProjectId">The created project identifier.</param>
/// <param name="ProjectName">The created project name.</param>
/// <param name="InfrastructureConfigId">The infrastructure configuration identifier when created.</param>
/// <param name="ResourceGroupId">The resource group identifier when created.</param>
/// <param name="InfrastructureError">The infrastructure creation error when project creation succeeded but infrastructure creation failed.</param>
/// <param name="CreatedResources">The resources created from the import preview.</param>
/// <param name="SkippedResources">The resources skipped during automatic creation.</param>
/// <param name="NextSuggestedActions">The suggested next steps.</param>
public sealed record ApplyImportPreviewResponse(
    string Status,
    string ProjectId,
    string ProjectName,
    string? InfrastructureConfigId,
    string? ResourceGroupId,
    string? InfrastructureError,
    IReadOnlyList<ApplyImportPreviewCreatedResourceResponseItem> CreatedResources,
    IReadOnlyList<ApplyImportPreviewSkippedResourceResponseItem> SkippedResources,
    IReadOnlyList<string> NextSuggestedActions);

/// <summary>
/// Represents a resource created from the import preview.
/// </summary>
/// <param name="ResourceType">The created resource type.</param>
/// <param name="ResourceId">The created resource identifier.</param>
/// <param name="Name">The created resource name.</param>
public sealed record ApplyImportPreviewCreatedResourceResponseItem(
    string ResourceType,
    string ResourceId,
    string Name);

/// <summary>
/// Represents a resource skipped during automatic creation.
/// </summary>
/// <param name="ResourceType">The skipped resource type.</param>
/// <param name="Name">The skipped resource name.</param>
/// <param name="Reason">The reason why automatic creation was skipped.</param>
public sealed record ApplyImportPreviewSkippedResourceResponseItem(
    string ResourceType,
    string Name,
    string Reason);