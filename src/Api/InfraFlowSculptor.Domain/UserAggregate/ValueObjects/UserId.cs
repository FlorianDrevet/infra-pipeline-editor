using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

public sealed class UserId(Guid value) : Id<UserId>(value);