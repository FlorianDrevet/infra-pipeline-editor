using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

public sealed class UserId(Guid value) : Id<UserId>(value);