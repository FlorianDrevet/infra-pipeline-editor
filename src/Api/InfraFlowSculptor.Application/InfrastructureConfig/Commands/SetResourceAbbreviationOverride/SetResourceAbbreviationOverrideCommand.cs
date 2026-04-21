using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;

/// <summary>
/// Sets (or updates) a per-resource-type abbreviation override.
/// The abbreviation is used for the <c>{resourceAbbr}</c> naming-template placeholder.
/// </summary>
public record SetResourceAbbreviationOverrideCommand(
    InfrastructureConfigId InfraConfigId,
    string ResourceType,
    string Abbreviation
) : ICommand<ResourceAbbreviationOverrideResult>;
