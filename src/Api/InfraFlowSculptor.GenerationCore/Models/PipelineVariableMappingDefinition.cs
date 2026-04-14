namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Maps a single pipeline variable from an Azure DevOps Library to a Bicep parameter name.
/// </summary>
public class PipelineVariableMappingDefinition
{
    /// <summary>Gets or sets the variable name in the Azure DevOps Library.</summary>
    public string PipelineVariableName { get; set; } = string.Empty;

    /// <summary>Gets or sets the target Bicep parameter name in main.bicep.</summary>
    public string BicepParameterName { get; set; } = string.Empty;
}
