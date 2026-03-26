using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

public record GetInfrastructureConfigResult(
    InfrastructureConfigId Id,
    Name Name,
    ProjectId ProjectId,
    string? DefaultNamingTemplate,
    bool UseProjectNamingConventions,
    IReadOnlyList<ResourceNamingTemplateResult> ResourceNamingTemplates,
    int ResourceGroupCount = 0,
    int ResourceCount = 0,
    int CrossConfigReferenceCount = 0);