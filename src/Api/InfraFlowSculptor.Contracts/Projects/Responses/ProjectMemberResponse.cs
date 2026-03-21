namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response representing a project member.</summary>
/// <param name="Id">Unique identifier of the membership.</param>
/// <param name="UserId">User identifier.</param>
/// <param name="EntraId">Azure Entra ID (OID) of the user.</param>
/// <param name="Role">Role assigned to this member.</param>
/// <param name="FirstName">First name of the user.</param>
/// <param name="LastName">Last name of the user.</param>
public record ProjectMemberResponse(
    Guid Id,
    Guid UserId,
    Guid EntraId,
    string Role,
    string FirstName,
    string LastName);
