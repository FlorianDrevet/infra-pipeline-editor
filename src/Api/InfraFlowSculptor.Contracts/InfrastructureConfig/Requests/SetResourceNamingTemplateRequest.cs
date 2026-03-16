using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

public class SetResourceNamingTemplateRequest
{
    /// <summary>
    /// The naming template string. Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
    /// </summary>
    [Required]
    public required string Template { get; init; }
}
