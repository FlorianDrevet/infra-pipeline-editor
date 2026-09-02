using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="NetworkingProfile"/>.</summary>
public sealed class NetworkingProfileId(Guid value) : Id<NetworkingProfileId>(value);
