using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Represents an application setting (environment variable) configured on a compute resource.
/// Can be a static value or a reference to another resource's output.
/// </summary>
public sealed class AppSetting : Entity<AppSettingId>
{
    /// <summary>Identifier of the Azure resource that owns this setting.</summary>
    public AzureResourceId ResourceId { get; private set; } = null!;

    /// <summary>The environment variable name (e.g., KEYVAULT_URI).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The static value for this setting. Null when this is a resource output reference.
    /// </summary>
    public string? StaticValue { get; private set; }

    /// <summary>
    /// Identifier of the source resource whose output is referenced.
    /// Null when this is a static value.
    /// </summary>
    public AzureResourceId? SourceResourceId { get; private set; }

    /// <summary>
    /// The output name from the source resource (e.g., "vaultUri", "connectionString").
    /// Null when this is a static value.
    /// </summary>
    public string? SourceOutputName { get; private set; }

    private AppSetting() { }

    /// <summary>Creates a new <see cref="AppSetting"/> with a static value.</summary>
    internal static AppSetting CreateStatic(
        AzureResourceId resourceId,
        string name,
        string value)
        => new()
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            StaticValue = value,
            SourceResourceId = null,
            SourceOutputName = null,
        };

    /// <summary>Creates a new <see cref="AppSetting"/> referencing another resource's output.</summary>
    internal static AppSetting CreateOutputReference(
        AzureResourceId resourceId,
        string name,
        AzureResourceId sourceResourceId,
        string sourceOutputName)
        => new()
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            StaticValue = null,
            SourceResourceId = sourceResourceId,
            SourceOutputName = sourceOutputName,
        };

    /// <summary>Updates this app setting to a static value.</summary>
    internal void UpdateToStatic(string name, string value)
    {
        Name = name;
        StaticValue = value;
        SourceResourceId = null;
        SourceOutputName = null;
    }

    /// <summary>Updates this app setting to reference a resource output.</summary>
    internal void UpdateToOutputReference(string name, AzureResourceId sourceResourceId, string sourceOutputName)
    {
        Name = name;
        StaticValue = null;
        SourceResourceId = sourceResourceId;
        SourceOutputName = sourceOutputName;
    }

    /// <summary>Gets whether this setting is a resource output reference.</summary>
    public bool IsOutputReference => SourceResourceId is not null;
}
