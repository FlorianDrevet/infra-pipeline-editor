namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Full representation of an Infrastructure Configuration.</summary>
/// <param name="Id">Unique identifier of the configuration.</param>
/// <param name="Name">Human-readable name of the configuration.</param>
/// <param name="DefaultNamingTemplate">
/// Default naming template applied to all resource types without a more specific override.
/// Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
/// Null when no default template is defined.
/// </param>
/// <param name="Members">List of users who have access to this configuration and their roles.</param>
/// <param name="EnvironmentDefinitions">List of target environments (e.g. Dev, Staging, Production).</param>
/// <param name="ResourceNamingTemplates">Per-resource-type naming template overrides.</param>
public record InfrastructureConfigResponse(
    string Id,
    string Name,
    string? DefaultNamingTemplate,
    IReadOnlyList<MemberResponse> Members,
    IReadOnlyList<EnvironmentDefinitionResponse> EnvironmentDefinitions,
    IReadOnlyList<ResourceNamingTemplateResponse> ResourceNamingTemplates);