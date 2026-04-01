using System;

namespace InfraFlowSculptor.BicepGeneration.Models;

public sealed record GeneratedTypeModule
{
    private string _moduleFileName = string.Empty;

    public string ModuleName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the generated file name for the primary resource module.
    /// Values assigned without the <c>.module.bicep</c> suffix are normalized automatically.
    /// </summary>
    public string ModuleFileName
    {
        get => _moduleFileName;
        init => _moduleFileName = NormalizePrimaryModuleFileName(value);
    }

    public string ModuleBicepContent { get; init; } = string.Empty;

    /// <summary>Content of the <c>types.bicep</c> file for this module (empty if no types).</summary>
    public string ModuleTypesBicepContent { get; init; } = string.Empty;

    /// <summary>The folder name under <c>modules/</c> (e.g. "KeyVault", "RedisCache").</summary>
    public string ModuleFolderName { get; init; } = string.Empty;

    public string ResourceGroupName { get; init; } = string.Empty;

    /// <summary>The logical resource name as configured by the user (e.g. "my-keyvault").</summary>
    public string LogicalResourceName { get; init; } = string.Empty;

    /// <summary>The short resource type abbreviation (e.g. "kv", "redis", "stg").</summary>
    public string ResourceAbbreviation { get; init; } = string.Empty;

    /// <summary>The simple resource type name used to look up naming templates (e.g. "KeyVault").</summary>
    public string ResourceTypeName { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, object> Parameters { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Optional companion modules to deploy alongside this resource
    /// (e.g. blob and table services for a Storage Account).
    /// </summary>
    public IReadOnlyList<GeneratedCompanionModule> CompanionModules { get; init; } = [];

    /// <summary>
    /// The identity kind for this specific resource instance.
    /// <c>None</c> = no identity needed, <c>SystemAssigned</c> / <c>UserAssigned</c> / <c>SystemAssigned, UserAssigned</c>.
    /// When <c>null</c>, identity is not managed by module parameterization.
    /// </summary>
    public string? IdentityKind { get; init; }

    /// <summary>
    /// Whether the module template uses parameterized identity
    /// (multiple identity types coexist for the same ARM resource type).
    /// When <c>true</c>, <c>main.bicep</c> must pass <c>identityType</c> and possibly UAI params.
    /// </summary>
    public bool UsesParameterizedIdentity { get; init; }

    /// <summary>
    /// Maps a Bicep parameter name in this module to the logical name of the parent resource
    /// whose module <c>outputs.id</c> should be passed.
    /// Example: <c>"appServicePlanId" → "my-asp"</c> generates
    /// <c>appServicePlanId: appServicePlanMyAspModule.outputs.id</c> in <c>main.bicep</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string> ParentModuleIdReferences { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Maps a Bicep parameter name in this module to the logical name of the parent resource
    /// whose computed naming expression should be passed.
    /// Used for child resources that need the parent's deployed name (e.g., SqlDatabase → sqlServerName).
    /// </summary>
    public IReadOnlyDictionary<string, string> ParentModuleNameReferences { get; init; } =
        new Dictionary<string, string>();

    private static string NormalizePrimaryModuleFileName(string moduleFileName)
    {
        if (string.IsNullOrWhiteSpace(moduleFileName))
        {
            return string.Empty;
        }

        if (moduleFileName.EndsWith(".module.bicep", StringComparison.OrdinalIgnoreCase))
        {
            return moduleFileName;
        }

        if (moduleFileName.EndsWith(".bicep", StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(
                moduleFileName.AsSpan(0, moduleFileName.Length - ".bicep".Length),
                ".module.bicep");
        }

        return $"{moduleFileName}.module.bicep";
    }
}
