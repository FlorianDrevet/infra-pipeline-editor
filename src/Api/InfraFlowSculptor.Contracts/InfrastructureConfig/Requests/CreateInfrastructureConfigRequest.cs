using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for creating a new Infrastructure Configuration.</summary>
public class CreateInfrastructureConfigRequest
{
    /// <summary>Human-readable name for the new Infrastructure Configuration.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Identifier of the parent project.</summary>
    [Required, GuidValidation]
    public required string ProjectId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
