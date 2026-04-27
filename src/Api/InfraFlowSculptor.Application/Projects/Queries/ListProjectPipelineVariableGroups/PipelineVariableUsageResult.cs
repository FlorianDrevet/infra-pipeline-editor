namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;

/// <summary>Result representing a single pipeline variable usage within a variable group.</summary>
/// <param name="PipelineVariableName">The name of the pipeline variable within the group.</param>
/// <param name="AppSettingName">The app setting, secure parameter, or app configuration key name.</param>
/// <param name="ResourceName">The display name of the Azure resource that owns the setting.</param>
/// <param name="ResourceType">The Azure resource type (e.g. "KeyVault", "WebApp").</param>
/// <param name="ConfigName">The display name of the infrastructure configuration the resource belongs to.</param>
public record PipelineVariableUsageResult(
    string PipelineVariableName,
    string AppSettingName,
    string ResourceName,
    string ResourceType,
    string ConfigName);
