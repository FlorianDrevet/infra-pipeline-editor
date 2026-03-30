using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;

/// <summary>
/// Represents the value of a static <see cref="AppConfigurationKey"/> for a specific deployment environment.
/// Each static configuration key has one value per project environment.
/// </summary>
public sealed class AppConfigurationKeyEnvironmentValue : Entity<AppConfigurationKeyEnvironmentValueId>
{
    /// <summary>Identifier of the parent <see cref="AppConfigurationKey"/>.</summary>
    public AppConfigurationKeyId AppConfigurationKeyId { get; private set; } = null!;

    /// <summary>The deployment environment name (e.g., "development", "production").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>The value for this environment.</summary>
    public string Value { get; private set; } = string.Empty;

    private AppConfigurationKeyEnvironmentValue() { }

    /// <summary>Creates a new <see cref="AppConfigurationKeyEnvironmentValue"/>.</summary>
    /// <param name="appConfigurationKeyId">Parent configuration key identifier.</param>
    /// <param name="environmentName">The environment name.</param>
    /// <param name="value">The value for this environment.</param>
    internal static AppConfigurationKeyEnvironmentValue Create(
        AppConfigurationKeyId appConfigurationKeyId,
        string environmentName,
        string value)
        => new()
        {
            Id = AppConfigurationKeyEnvironmentValueId.CreateUnique(),
            AppConfigurationKeyId = appConfigurationKeyId,
            EnvironmentName = environmentName,
            Value = value,
        };

    /// <summary>Updates the value for this environment.</summary>
    /// <param name="value">The new value.</param>
    internal void UpdateValue(string value)
    {
        Value = value;
    }
}
