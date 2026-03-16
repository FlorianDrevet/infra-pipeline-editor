using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

public class SetDefaultNamingTemplateRequest
{
    /// <summary>
    /// The naming template string. Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
    /// Set to null to clear the default template.
    /// </summary>
    public string? Template { get; init; }
}
