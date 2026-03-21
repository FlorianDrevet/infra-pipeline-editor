namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Represents a member of an Infrastructure Configuration with their assigned role.</summary>
/// <param name="Id">Unique identifier of the membership record.</param>
/// <param name="UserId">User identifier.</param>
/// <param name="EntraId">Azure Entra Object ID of the user.</param>
/// <param name="Role">Role assigned to the user (e.g. "Owner", "Contributor", "Reader").</param>
/// <param name="FirstName">First name of the user.</param>
/// <param name="LastName">Last name of the user.</param>
public record MemberResponse(Guid Id, Guid UserId, Guid EntraId, string Role, string FirstName, string LastName);
