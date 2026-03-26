using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result representing a project.</summary>
public record ProjectResult(
    ProjectId Id,
    Name Name,
    string? Description,
    IReadOnlyList<ProjectMemberResult> Members,
    IReadOnlyList<ProjectEnvironmentDefinitionResult> EnvironmentDefinitions,
    string? DefaultNamingTemplate,
    IReadOnlyList<ProjectResourceNamingTemplateResult> ResourceNamingTemplates,
    GitRepositoryConfigurationResult? GitRepositoryConfiguration,
    string RepositoryMode);
