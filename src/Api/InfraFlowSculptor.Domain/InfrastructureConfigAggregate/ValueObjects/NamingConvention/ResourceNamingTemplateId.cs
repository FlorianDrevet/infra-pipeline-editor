using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class ResourceNamingTemplateId(Guid value) : Id<ResourceNamingTemplateId>(value);
