namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Full representation of an Infrastructure Configuration.</summary>
/// <param name="Id">Unique identifier of the configuration.</param>
/// <param name="Name">Human-readable name of the configuration.</param>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="DefaultNamingTemplate">
/// Default naming template applied to all resource types without a more specific override.
/// Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
/// Null when no default template is defined.
/// </param>
/// <param name="UseProjectEnvironments">Whether this config inherits environments from the parent project.</param>
/// <param name="UseProjectNamingConventions">Whether this config inherits naming conventions from the parent project.</param>
/// <param name="EnvironmentDefinitions">List of target environments (e.g. Dev, Staging, Production).</param>
/// <param name="ResourceNamingTemplates">Per-resource-type naming template overrides.</param>
public record InfrastructureConfigResponse(
    string Id,
    string Name,
    string ProjectId,
    string? DefaultNamingTemplate,
    bool UseProjectEnvironments,
    bool UseProjectNamingConventions,
    IReadOnlyList<EnvironmentDefinitionResponse> EnvironmentDefinitions,
    IReadOnlyList<ResourceNamingTemplateResponse> ResourceNamingTemplates);