using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;

/// <summary>
/// Returns file listings of the latest generated artifacts (Bicep, Pipeline, Bootstrap) for a project
/// without re-generating. Returns null sections when no generation exists for that artifact type.
/// </summary>
public record GetProjectLatestGenerationQuery(
    Guid ProjectId
) : IQuery<GetProjectLatestGenerationResult>;
