using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Full representation of a project.</summary>
public record ProjectResponse(
    string Id,
    string Name,
    string? Description,
    IReadOnlyList<ProjectMemberResponse> Members,
    IReadOnlyList<EnvironmentDefinitionResponse> EnvironmentDefinitions,
    string? DefaultNamingTemplate,
    IReadOnlyList<ResourceNamingTemplateResponse> ResourceNamingTemplates,
    IReadOnlyList<ResourceAbbreviationOverrideResponse> ResourceAbbreviations,
    IReadOnlyList<TagResponse> Tags,
    string? AgentPoolName = null,
    IReadOnlyList<string>? UsedResourceTypes = null,
    IReadOnlyList<ProjectRepositoryResponse>? Repositories = null,
    string? LayoutPreset = null,
    string? CommonsStrategy = null);
