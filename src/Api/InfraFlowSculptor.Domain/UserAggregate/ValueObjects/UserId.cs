using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="User"/>.</summary>
public sealed class UserId(Guid value) : Id<UserId>(value);