using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for creating a new Infrastructure Configuration.</summary>
public class CreateInfrastructureConfigRequest
{
    /// <summary>Human-readable name for the new Infrastructure Configuration.</summary>
    [Required]
    public required string Name { get; init; }
}