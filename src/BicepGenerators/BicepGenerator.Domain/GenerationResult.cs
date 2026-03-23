namespace BicepGenerator.Domain;

public sealed class GenerationResult
{
    public string MainBicep { get; init; }

    /// <summary>
    /// Per-environment parameter files. Key = file name (e.g. <c>main.dev.bicepparam</c>), Value = file content.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentParameterFiles { get; init; } =
        new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> ModuleFiles { get; init; }
}
