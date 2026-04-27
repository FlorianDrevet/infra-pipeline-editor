using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result for a project-level resource abbreviation override.</summary>
public record ProjectResourceAbbreviationResult(
    ProjectResourceAbbreviationId Id,
    string ResourceType,
    string Abbreviation);
