namespace BicepGenerator.Domain;

public sealed class GenerationResult
{
    public string MainBicep { get; init; }
    public string MainBicepParameters { get; init; }
    public IReadOnlyDictionary<string, string> ModuleFiles { get; init; }
}
