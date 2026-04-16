using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result representing a project.</summary>
public record ProjectResult(
    ProjectId Id,
    Name Name,
    string? Description,
    IReadOnlyCollection<ProjectMemberResult> Members,
    IReadOnlyCollection<ProjectEnvironmentDefinitionResult> EnvironmentDefinitions,
    string? DefaultNamingTemplate,
    IReadOnlyCollection<ProjectResourceNamingTemplateResult> ResourceNamingTemplates,
    GitRepositoryConfigurationResult? GitRepositoryConfiguration,
    string RepositoryMode,
    IReadOnlyCollection<TagResult> Tags,
    string? AgentPoolName = null);
