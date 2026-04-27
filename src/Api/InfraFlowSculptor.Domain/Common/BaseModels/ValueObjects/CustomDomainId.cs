using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entites.CustomDomain"/>.</summary>
public sealed class CustomDomainId(Guid value) : Id<CustomDomainId>(value);
