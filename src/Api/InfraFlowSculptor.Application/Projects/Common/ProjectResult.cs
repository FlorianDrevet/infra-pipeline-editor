using InfraFlowSculptor.Application.InfrastructureConfig.Common;
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
    IReadOnlyList<ProjectResourceAbbreviationResult> ResourceAbbreviations,
    IReadOnlyList<TagResult> Tags,
    string? AgentPoolName = null,
    IReadOnlyList<string>? UsedResourceTypes = null,
    IReadOnlyList<ProjectRepositoryResult>? Repositories = null,
    string LayoutPreset = "MultiRepo");
