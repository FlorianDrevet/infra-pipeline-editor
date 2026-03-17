using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

/// <summary>Request body for creating a new Storage Account resource inside a Resource Group.</summary>
public class CreateStorageAccountRequest : StorageAccountRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Storage Account.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
