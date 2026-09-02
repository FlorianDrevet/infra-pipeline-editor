using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.DocumentIntelligences.Requests;

/// <summary>Request body for updating an existing Document Intelligence resource.</summary>
public class UpdateDocumentIntelligenceRequest
{
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
}
