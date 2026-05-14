using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.FrontDoorOrigin"/>.</summary>
public sealed class FrontDoorOriginId(Guid value) : Id<FrontDoorOriginId>(value);
