using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.NetworkingProfileEnvironmentOverride"/>.</summary>
public sealed class NetworkingProfileEnvironmentOverrideId(Guid value) : Id<NetworkingProfileEnvironmentOverrideId>(value);
