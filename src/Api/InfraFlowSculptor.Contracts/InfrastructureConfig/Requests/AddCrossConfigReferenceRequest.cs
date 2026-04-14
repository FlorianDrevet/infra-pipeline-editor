using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for adding a cross-configuration resource reference.</summary>
public class AddCrossConfigReferenceRequest
{
    /// <summary>Identifier of the target Azure resource to reference.</summary>
    [Required]
    public required Guid TargetResourceId { get; init; }
}
