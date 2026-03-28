using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.UserAssignedIdentities.Requests;

/// <summary>
/// Request body for unlinking a source resource from a User-Assigned Identity.
/// The role assignments are preserved on the UAI; only the consuming resource's association is removed.
/// </summary>
public sealed class UnlinkResourceFromIdentityRequest
{
    /// <summary>The identifier of the source resource to unlink.</summary>
    [Required]
    public Guid SourceResourceId { get; init; }
}
