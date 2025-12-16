namespace BicepGenerator.Domain;

public sealed class GeneratedTypeModule
{
    public string ModuleName { get; init; }
    public string ModuleFileName { get; init; }
    public string ModuleBicepContent { get; init; }

    public IReadOnlyDictionary<string, object> Parameters { get; init; }
}