using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.SecureParameterMappings.Requests;

/// <summary>Request body for setting a secure parameter mapping.</summary>
public class SetSecureParameterMappingRequest
{
    /// <summary>Name of the secure parameter (e.g. <c>administratorLoginPassword</c>).</summary>
    [Required]
    public required string SecureParameterName { get; init; }

    /// <summary>Optional pipeline variable group identifier. <c>null</c> to clear the mapping.</summary>
    public Guid? VariableGroupId { get; init; }

    /// <summary>Optional pipeline variable name within the group. Required when VariableGroupId is set.</summary>
    [StringLength(200)]
    public string? PipelineVariableName { get; init; }
}
