namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Lightweight projection of a project for list/validation queries.</summary>
/// <param name="Id">The project identifier as a raw GUID.</param>
/// <param name="Name">The project display name.</param>
/// <param name="Description">The optional project description.</param>
public sealed record ProjectSummary(Guid Id, string Name, string? Description);
