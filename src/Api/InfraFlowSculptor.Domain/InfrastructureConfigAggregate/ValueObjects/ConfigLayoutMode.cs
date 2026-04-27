using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>
/// Defines the per-configuration repository layout when the parent project is in
/// <see cref="ProjectAggregate.ValueObjects.LayoutPresetEnum.MultiRepo"/> mode.
/// </summary>
public enum ConfigLayoutModeEnum
{
    /// <summary>One single repository hosts both infrastructure and application code for this configuration.</summary>
    AllInOne,

    /// <summary>Two repositories: one for infrastructure, one for application code.</summary>
    SplitInfraCode,
}

/// <summary>Value object wrapping <see cref="ConfigLayoutModeEnum"/>.</summary>
public sealed class ConfigLayoutMode(ConfigLayoutModeEnum value) : EnumValueObject<ConfigLayoutModeEnum>(value);
