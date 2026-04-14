using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Defines how generated Bicep files are organized relative to Git repositories.</summary>
public enum RepositoryModeEnum
{
    /// <summary>Each configuration has its own repository (or branch). Git config and push are per-configuration.</summary>
    MultiRepo,

    /// <summary>All configurations share a single repository. Git config and push are at project level.</summary>
    MonoRepo,
}

/// <summary>Value object wrapping <see cref="RepositoryModeEnum"/>.</summary>
public sealed class RepositoryMode(RepositoryModeEnum value) : EnumValueObject<RepositoryModeEnum>(value);
