using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

/// <summary>Request body for adding a Storage Queue to a Storage Account.</summary>
public class AddQueueRequest
{
    /// <summary>Display name for the Storage Queue.</summary>
    [Required]
    public required string Name { get; init; }
}
