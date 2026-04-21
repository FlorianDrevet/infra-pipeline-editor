using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceAbbreviationOverride;

/// <summary>Removes a per-resource-type abbreviation override from a configuration.</summary>
public record RemoveResourceAbbreviationOverrideCommand(
    InfrastructureConfigId InfraConfigId,
    string ResourceType
) : ICommand<Deleted>;
