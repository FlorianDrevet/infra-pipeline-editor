using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

/// <summary>Request body for updating the public access level of a blob container.</summary>
public class UpdateBlobContainerPublicAccessRequest
{
    /// <summary>
    /// The new public access level for the blob container.
    /// Accepted values: <c>None</c>, <c>Blob</c>, <c>Container</c>.
    /// </summary>
    [Required, EnumValidation(typeof(BlobContainerPublicAccess.AccessLevel))]
    public required string PublicAccess { get; init; }
}
