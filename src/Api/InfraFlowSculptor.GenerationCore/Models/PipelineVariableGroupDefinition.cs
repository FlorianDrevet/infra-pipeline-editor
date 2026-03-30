namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents an Azure DevOps Pipeline Variable Group and its variable-to-Bicep-parameter mappings,
/// used during pipeline YAML generation to emit <c>- group:</c> references and <c>overrideParameters</c>.
/// </summary>
public class PipelineVariableGroupDefinition
{
    /// <summary>Gets or sets the name of the Azure DevOps Variable Group.</summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>Gets or sets the variable-to-Bicep-parameter mappings.</summary>
    public IReadOnlyList<PipelineVariableMappingDefinition> Mappings { get; set; } = [];
}
