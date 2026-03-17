namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Represents a member of an Infrastructure Configuration with their assigned role.</summary>
/// <param name="Id">Unique identifier of the membership record.</param>
/// <param name="UserId">User identifier.</param>
/// <param name="Role">Role assigned to the user (e.g. "Owner", "Contributor", "Reader").</param>
public record MemberResponse(Guid Id, Guid UserId, string Role);
