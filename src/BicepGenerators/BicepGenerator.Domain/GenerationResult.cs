namespace BicepGenerator.Domain;

public sealed class GenerationResult
{
    /// <summary>Content of <c>main.bicep</c> — the main deployment orchestration file.</summary>
    public string MainBicep { get; init; } = string.Empty;

    /// <summary>Content of <c>types.bicep</c> — exported environment types and variables.</summary>
    public string TypesBicep { get; init; } = string.Empty;

    /// <summary>Content of <c>functions.bicep</c> — exported naming functions.</summary>
    public string FunctionsBicep { get; init; } = string.Empty;

    /// <summary>
    /// Per-environment parameter files. Key = file name (e.g. <c>main.dev.bicepparam</c>), Value = file content.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentParameterFiles { get; init; } =
        new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> ModuleFiles { get; init; } =
        new Dictionary<string, string>();
}
