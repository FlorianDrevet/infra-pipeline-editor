using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

public record MemberResult(MemberId Id, UserId UserId, string Role);
