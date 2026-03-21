using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>Application-layer result representing a member with resolved user name.</summary>
/// <param name="Id">Unique identifier of the membership record.</param>
/// <param name="UserId">User identifier.</param>
/// <param name="Role">Role assigned to the user.</param>
/// <param name="FirstName">First name of the user.</param>
/// <param name="LastName">Last name of the user.</param>
public record MemberResult(MemberId Id, UserId UserId, string Role, string FirstName, string LastName);
