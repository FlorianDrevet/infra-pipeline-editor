using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entites.SecureParameterMapping"/>.</summary>
public sealed class SecureParameterMappingId(Guid value) : Id<SecureParameterMappingId>(value);
