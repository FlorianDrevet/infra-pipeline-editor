namespace InfraFlowSculptor.BicepGeneration.Models;

public sealed record GeneratedTypeModule
{
    public string ModuleName { get; init; } = string.Empty;
    public string ModuleFileName { get; init; } = string.Empty;
    public string ModuleBicepContent { get; init; } = string.Empty;
    public string ResourceGroupName { get; init; } = string.Empty;

    /// <summary>The logical resource name as configured by the user (e.g. "my-keyvault").</summary>
    public string LogicalResourceName { get; init; } = string.Empty;

    /// <summary>The short resource type abbreviation (e.g. "kv", "redis", "stg").</summary>
    public string ResourceAbbreviation { get; init; } = string.Empty;

    /// <summary>The simple resource type name used to look up naming templates (e.g. "KeyVault").</summary>
    public string ResourceTypeName { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, object> Parameters { get; init; } =
        new Dictionary<string, object>();
}
