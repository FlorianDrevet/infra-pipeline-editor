using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for setting or replacing the naming template for a specific Azure resource type.</summary>
public class SetResourceNamingTemplateRequest
{
    /// <summary>
    /// The naming template string. Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
    /// </summary>
    [Required]
    public required string Template { get; init; }
}
