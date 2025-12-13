using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

public sealed class ResourceGroupId(Guid value) : Id<ResourceGroupId>(value);