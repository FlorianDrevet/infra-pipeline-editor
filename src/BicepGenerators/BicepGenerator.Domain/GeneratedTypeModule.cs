namespace BicepGenerator.Domain;

public sealed record GeneratedTypeModule
{
    public string ModuleName { get; init; } = string.Empty;
    public string ModuleFileName { get; init; } = string.Empty;
    public string ModuleBicepContent { get; init; } = string.Empty;
    public string ResourceGroupName { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object> Parameters { get; init; } =
        new Dictionary<string, object>();
}