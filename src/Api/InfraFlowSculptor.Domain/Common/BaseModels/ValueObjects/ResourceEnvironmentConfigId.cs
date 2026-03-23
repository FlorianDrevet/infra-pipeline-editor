using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entites.ResourceEnvironmentConfig"/>.</summary>
public sealed class ResourceEnvironmentConfigId(Guid value) : Id<ResourceEnvironmentConfigId>(value);
