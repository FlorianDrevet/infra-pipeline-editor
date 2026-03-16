using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

public record ResourceNamingTemplateResult(
    ResourceNamingTemplateId Id,
    string ResourceType,
    string Template);
