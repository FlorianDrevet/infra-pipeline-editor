using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>Application-layer result representing a registered user.</summary>
/// <param name="Id">Unique identifier of the user.</param>
/// <param name="FirstName">First name of the user.</param>
/// <param name="LastName">Last name of the user.</param>
public record UserResult(UserId Id, string FirstName, string LastName);
