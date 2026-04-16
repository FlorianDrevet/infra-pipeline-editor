using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Full representation of a project.</summary>
public record ProjectResponse(
    string Id,
    string Name,
    string? Description,
    IReadOnlyCollection<ProjectMemberResponse> Members,
    IReadOnlyCollection<EnvironmentDefinitionResponse> EnvironmentDefinitions,
    string? DefaultNamingTemplate,
    IReadOnlyCollection<ResourceNamingTemplateResponse> ResourceNamingTemplates,
    GitConfigResponse? GitRepositoryConfiguration,
    string RepositoryMode,
    IReadOnlyCollection<TagResponse> Tags,
    string? AgentPoolName = null);
