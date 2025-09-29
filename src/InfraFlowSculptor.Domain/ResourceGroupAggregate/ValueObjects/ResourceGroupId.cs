using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

public sealed class ResourceGroupId(Guid value) : Id<ResourceGroupId>(value);