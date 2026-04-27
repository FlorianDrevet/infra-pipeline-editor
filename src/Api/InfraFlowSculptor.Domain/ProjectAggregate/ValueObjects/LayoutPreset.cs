using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Defines the high-level repository layout strategy of a project.</summary>
public enum LayoutPresetEnum
{
    /// <summary>One single repository contains infrastructure, application code and all pipelines.</summary>
    AllInOne,

    /// <summary>Two repositories: one for infrastructure (with infra pipelines), one for application code (with app pipelines).</summary>
    SplitInfraCode,

    /// <summary>Repositories are declared per infrastructure configuration. The project itself owns no repository.</summary>
    MultiRepo,
}

/// <summary>Value object wrapping <see cref="LayoutPresetEnum"/>.</summary>
public sealed class LayoutPreset(LayoutPresetEnum value) : EnumValueObject<LayoutPresetEnum>(value);
