namespace InfraFlowSculptor.Contracts.SecureParameterMappings.Responses;

/// <summary>Response DTO for a secure parameter mapping.</summary>
/// <param name="Id">Mapping identifier.</param>
/// <param name="SecureParameterName">Name of the secure Bicep parameter.</param>
/// <param name="VariableGroupId">Pipeline variable group identifier, or <c>null</c>.</param>
/// <param name="VariableGroupName">Pipeline variable group display name, or <c>null</c>.</param>
/// <param name="PipelineVariableName">Variable name within the group, or <c>null</c>.</param>
public record SecureParameterMappingResponse(
    string Id,
    string SecureParameterName,
    string? VariableGroupId,
    string? VariableGroupName,
    string? PipelineVariableName);
