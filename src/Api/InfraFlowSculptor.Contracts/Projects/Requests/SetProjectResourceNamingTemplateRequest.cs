using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for setting or replacing a per-resource-type naming template at the project level.</summary>
public class SetProjectResourceNamingTemplateRequest
{
    /// <summary>
    /// The naming template string. Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
    /// </summary>
    [Required]
    public required string Template { get; init; }
}
