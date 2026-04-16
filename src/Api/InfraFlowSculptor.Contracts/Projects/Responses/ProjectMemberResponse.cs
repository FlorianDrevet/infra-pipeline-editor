namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Representation of a project member.</summary>
public record ProjectMemberResponse(
    string Id,
    string UserId,
    string EntraId,
    string Role,
    string FirstName,
    string LastName);
