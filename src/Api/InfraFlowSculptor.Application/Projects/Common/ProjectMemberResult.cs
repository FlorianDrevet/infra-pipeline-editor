using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result representing a project member with resolved user info.</summary>
public record ProjectMemberResult(
    ProjectMemberId Id,
    UserId UserId,
    Guid EntraId,
    string Role,
    string FirstName,
    string LastName);
