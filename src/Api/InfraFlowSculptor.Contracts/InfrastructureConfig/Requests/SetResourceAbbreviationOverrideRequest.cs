using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for setting or replacing the abbreviation for a specific Azure resource type.</summary>
public class SetResourceAbbreviationOverrideRequest
{
    /// <summary>
    /// The abbreviation string. Must be lowercase alphanumeric, max 10 characters.
    /// </summary>
    [Required]
    [MaxLength(10)]
    [RegularExpression(@"^[a-z0-9]+$", ErrorMessage = "Abbreviation must be lowercase alphanumeric.")]
    public required string Abbreviation { get; init; }
}
