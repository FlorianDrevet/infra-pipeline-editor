using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;

public sealed class ResourceParameterUsageId(Guid value) : Id<ResourceParameterUsageId>(value);