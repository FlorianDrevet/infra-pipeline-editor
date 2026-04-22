namespace InfraFlowSculptor.Application.SecureParameterMappings.Common;

/// <summary>Application-layer result for a secure parameter mapping.</summary>
public sealed record SecureParameterMappingResult(
    Guid Id,
    string SecureParameterName,
    Guid? VariableGroupId,
    string? VariableGroupName,
    string? PipelineVariableName);
