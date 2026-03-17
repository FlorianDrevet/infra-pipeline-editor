namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>
/// Request body for setting or clearing the default naming template applied to all resource types
/// that do not have a more specific per-resource-type override.
/// </summary>
public class SetDefaultNamingTemplateRequest
{
    /// <summary>
    /// The naming template string. Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
    /// Set to <c>null</c> to clear the default template (fall back to no template).
    /// </summary>
    public string? Template { get; init; }
}
