using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Represents the value of a static <see cref="AppSetting"/> for a specific deployment environment.
/// Each static app setting has one value per project environment.
/// </summary>
public sealed class AppSettingEnvironmentValue : Entity<AppSettingEnvironmentValueId>
{
    /// <summary>Identifier of the parent <see cref="AppSetting"/>.</summary>
    public AppSettingId AppSettingId { get; private set; } = null!;

    /// <summary>The deployment environment name (e.g., "development", "production").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>The value for this environment.</summary>
    public string Value { get; private set; } = string.Empty;

    private AppSettingEnvironmentValue() { }

    /// <summary>Creates a new <see cref="AppSettingEnvironmentValue"/>.</summary>
    /// <param name="appSettingId">Parent app setting identifier.</param>
    /// <param name="environmentName">The environment name.</param>
    /// <param name="value">The value for this environment.</param>
    internal static AppSettingEnvironmentValue Create(
        AppSettingId appSettingId,
        string environmentName,
        string value)
        => new()
        {
            Id = AppSettingEnvironmentValueId.CreateUnique(),
            AppSettingId = appSettingId,
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
