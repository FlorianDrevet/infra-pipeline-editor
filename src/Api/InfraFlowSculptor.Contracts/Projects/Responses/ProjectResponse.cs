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
    GitConfigResponse? GitRepositoryConfiguration,
    string RepositoryMode,
    IReadOnlyList<TagResponse> Tags);
