using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

public record GetInfrastructureConfigResult(
    InfrastructureConfigId Id,
    Name Name,
    string? DefaultNamingTemplate,
    IReadOnlyList<MemberResult> Members,
    IReadOnlyList<EnvironmentDefinitionResult> EnvironmentDefinitions,
    IReadOnlyList<ResourceNamingTemplateResult> ResourceNamingTemplates);