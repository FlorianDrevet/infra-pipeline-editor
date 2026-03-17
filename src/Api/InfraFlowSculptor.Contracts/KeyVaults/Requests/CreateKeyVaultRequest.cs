using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.KeyVaults.Requests;

/// <summary>Request body for creating a new Key Vault resource inside a Resource Group.</summary>
public class CreateKeyVaultRequest : KeyVaultRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Key Vault.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}