using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>Application-layer result for a config-level resource abbreviation override.</summary>
public record ResourceAbbreviationOverrideResult(
    ResourceAbbreviationOverrideId Id,
    string ResourceType,
    string Abbreviation);
