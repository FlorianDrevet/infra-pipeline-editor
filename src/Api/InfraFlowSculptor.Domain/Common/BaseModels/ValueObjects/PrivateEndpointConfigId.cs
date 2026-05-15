using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entites.PrivateEndpointConfig"/>.</summary>
public sealed class PrivateEndpointConfigId(Guid value) : Id<PrivateEndpointConfigId>(value);
