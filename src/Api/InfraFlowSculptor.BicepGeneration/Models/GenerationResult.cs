using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Models;

public sealed class GenerationResult : IGenerationResult
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

    /// <summary>Role assignments used to build <c>constants.bicep</c> for this config.</summary>
    public IReadOnlyList<RoleAssignmentDefinition> RoleAssignments { get; init; } = [];

    /// <summary>Content of <c>constants.bicep</c> — exported RBAC role definitions grouped by service.</summary>
    public string ConstantsBicep { get; init; } = string.Empty;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Files
    {
        get
        {
            var files = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(MainBicep))
                files["main.bicep"] = MainBicep;
            if (!string.IsNullOrEmpty(TypesBicep))
                files["types.bicep"] = TypesBicep;
            if (!string.IsNullOrEmpty(FunctionsBicep))
                files["functions.bicep"] = FunctionsBicep;
            if (!string.IsNullOrEmpty(ConstantsBicep))
                files["constants.bicep"] = ConstantsBicep;

            foreach (var (fileName, content) in EnvironmentParameterFiles)
                files[$"parameters/{fileName}"] = content;

            foreach (var (path, content) in ModuleFiles)
                files[path] = content;

            return files;
        }
    }
}
