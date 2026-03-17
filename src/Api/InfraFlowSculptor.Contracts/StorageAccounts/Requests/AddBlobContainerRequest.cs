using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

/// <summary>Request body for adding a Blob Container to a Storage Account.</summary>
public class AddBlobContainerRequest
{
    /// <summary>Display name for the Blob Container.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>
    /// Public access level for blobs in this container.
    /// Accepted values: <c>None</c>, <c>Blob</c>, <c>Container</c>.
    /// </summary>
    [Required, EnumValidation(typeof(BlobContainerPublicAccess.AccessLevel))]
    public required string PublicAccess { get; init; }
}
