using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

public record GetInfrastructureConfigResult(
    InfrastructureConfigId Id,
    Name Name,
    ProjectId ProjectId,
    string? DefaultNamingTemplate,
    IReadOnlyList<EnvironmentDefinitionResult> EnvironmentDefinitions,
    IReadOnlyList<ResourceNamingTemplateResult> ResourceNamingTemplates);