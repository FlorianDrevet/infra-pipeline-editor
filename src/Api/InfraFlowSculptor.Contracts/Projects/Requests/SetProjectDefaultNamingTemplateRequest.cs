namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>
/// Request body for setting or clearing the default naming template at the project level.
/// Applied to all resource types that do not have a more specific per-resource-type override.
/// </summary>
public class SetProjectDefaultNamingTemplateRequest
{
    /// <summary>
    /// The naming template string. Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
    /// Set to <c>null</c> to clear the default template.
    /// </summary>
    public string? Template { get; init; }
}
