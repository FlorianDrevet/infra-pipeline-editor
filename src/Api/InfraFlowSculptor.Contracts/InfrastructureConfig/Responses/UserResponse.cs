namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Represents a registered user available for membership assignment.</summary>
/// <param name="Id">Unique identifier of the user.</param>
/// <param name="FirstName">First name of the user.</param>
/// <param name="LastName">Last name of the user.</param>
public record UserResponse(Guid Id, string FirstName, string LastName);
