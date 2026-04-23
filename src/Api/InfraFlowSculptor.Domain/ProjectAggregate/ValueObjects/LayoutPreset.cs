using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Defines the high-level repository layout strategy of a project.</summary>
public enum LayoutPresetEnum
{
    /// <summary>One single repository contains infrastructure, application code, and pipelines.</summary>
    AllInOne,

    /// <summary>Two repositories: one for infrastructure, one for application code.</summary>
    SplitInfraCode,

    /// <summary>N repositories, typically one per infrastructure configuration.</summary>
    MultiRepo,

    /// <summary>Custom mix of repositories defined manually by the user.</summary>
    Custom,
}

/// <summary>Value object wrapping <see cref="LayoutPresetEnum"/>.</summary>
public sealed class LayoutPreset(LayoutPresetEnum value) : EnumValueObject<LayoutPresetEnum>(value);
