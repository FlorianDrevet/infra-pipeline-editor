namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Full representation of a project.</summary>
public record ProjectResponse(
    string Id,
    string Name,
    string? Description,
    IReadOnlyList<ProjectMemberResponse> Members);
