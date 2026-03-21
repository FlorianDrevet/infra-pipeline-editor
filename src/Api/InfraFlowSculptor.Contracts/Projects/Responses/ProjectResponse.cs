namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Full representation of a project.</summary>
/// <param name="Id">Unique identifier of the project.</param>
/// <param name="Name">Human-readable name of the project.</param>
/// <param name="Description">Optional description of the project.</param>
/// <param name="Members">List of users who have access to this project and their roles.</param>
public record ProjectResponse(
    string Id,
    string Name,
    string? Description,
    IReadOnlyList<ProjectMemberResponse> Members);
