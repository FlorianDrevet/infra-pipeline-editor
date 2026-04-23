using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to set the commons strategy of a project. V1 only accepts <c>DuplicatePerRepo</c>.</summary>
public sealed class SetProjectCommonsStrategyRequest
{
    /// <summary>Strategy name. V1 only accepts <c>DuplicatePerRepo</c>.</summary>
    [Required]
    public required string Strategy { get; init; }
}
