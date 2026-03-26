using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for adding a cross-configuration resource reference.</summary>
public class AddCrossConfigReferenceRequest
{
    /// <summary>Identifier of the target Azure resource to reference.</summary>
    [Required, GuidValidation]
    public required string TargetResourceId { get; init; }
}
