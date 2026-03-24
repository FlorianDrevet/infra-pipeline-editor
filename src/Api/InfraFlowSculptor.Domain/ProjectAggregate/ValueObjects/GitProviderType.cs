using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Supported Git hosting providers.</summary>
public enum GitProviderTypeEnum
{
    /// <summary>GitHub.com or GitHub Enterprise.</summary>
    GitHub,

    /// <summary>Azure DevOps Services or Server.</summary>
    AzureDevOps,
}

/// <summary>Value object wrapping <see cref="GitProviderTypeEnum"/>.</summary>
public sealed class GitProviderType(GitProviderTypeEnum value) : EnumValueObject<GitProviderTypeEnum>(value);
