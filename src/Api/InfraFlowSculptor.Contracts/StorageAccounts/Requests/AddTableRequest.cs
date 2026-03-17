using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

/// <summary>Request body for adding a Storage Table to a Storage Account.</summary>
public class AddTableRequest
{
    /// <summary>Display name for the Storage Table.</summary>
    [Required]
    public required string Name { get; init; }
}
