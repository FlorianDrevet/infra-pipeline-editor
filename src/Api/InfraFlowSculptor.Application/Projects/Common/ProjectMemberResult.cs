using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result for a project member.</summary>
public record ProjectMemberResult(
    ProjectMemberId Id,
    UserId UserId,
    Guid EntraId,
    string Role,
    string FirstName,
    string LastName);
