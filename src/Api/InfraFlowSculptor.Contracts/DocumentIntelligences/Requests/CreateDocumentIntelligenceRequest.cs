using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.DocumentIntelligences.Requests;

/// <summary>Request body for creating a new Document Intelligence resource.</summary>
public class CreateDocumentIntelligenceRequest
{
    /// <summary>Unique identifier of the Resource Group that will own this Document Intelligence resource.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>User-friendly name for the Document Intelligence resource.</summary>
    [Required, MaxLength(256)]
    public required string Name { get; init; }

    /// <summary>Azure region where the resource will be deployed.</summary>
    [Required, MaxLength(100)]
    public required string Location { get; init; }

    /// <summary>Optional custom sub-domain name for the Cognitive Services account.</summary>
    [MaxLength(64)]
    public string? CustomSubDomainName { get; init; }

    /// <summary>Per-environment configuration settings.</summary>
    public List<DocumentIntelligenceEnvironmentConfigRequest>? EnvironmentSettings { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>
    public bool IsExisting { get; init; } = false;
}
