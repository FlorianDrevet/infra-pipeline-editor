using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;

/// <summary>
/// Sets (or updates) a per-resource-type naming template override.
/// ResourceType is the type name (e.g. "KeyVault", "StorageAccount").
/// Template uses placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
/// </summary>
public record SetResourceNamingTemplateCommand(
    InfrastructureConfigId InfraConfigId,
    string ResourceType,
    string Template
) : IRequest<ErrorOr<ResourceNamingTemplateResult>>;
