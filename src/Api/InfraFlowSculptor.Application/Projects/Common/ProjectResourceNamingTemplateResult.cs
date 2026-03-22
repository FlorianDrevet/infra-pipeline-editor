using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result for a project-level per-resource-type naming template.</summary>
public record ProjectResourceNamingTemplateResult(
    ProjectResourceNamingTemplateId Id,
    string ResourceType,
    string Template);
