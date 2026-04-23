using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Defines how shared/common Bicep modules are distributed across repositories.</summary>
public enum CommonsStrategyEnum
{
    /// <summary>Common modules are duplicated into each repository (V1 default and only supported strategy).</summary>
    DuplicatePerRepo,

    /// <summary>Common modules live in a dedicated commons repository (planned for V3).</summary>
    DedicatedCommonsRepo,

    /// <summary>Common modules are referenced via an Azure DevOps repository resource (planned for V3).</summary>
    AzdoRepoResource,
}

/// <summary>Value object wrapping <see cref="CommonsStrategyEnum"/>.</summary>
public sealed class CommonsStrategy(CommonsStrategyEnum value) : EnumValueObject<CommonsStrategyEnum>(value);
