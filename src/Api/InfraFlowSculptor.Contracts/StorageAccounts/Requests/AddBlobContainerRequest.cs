using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

public class AddBlobContainerRequest
{
    [Required]
    public required string Name { get; init; }

    [Required, EnumValidation(typeof(BlobContainerPublicAccess.AccessLevel))]
    public required string PublicAccess { get; init; }
}
