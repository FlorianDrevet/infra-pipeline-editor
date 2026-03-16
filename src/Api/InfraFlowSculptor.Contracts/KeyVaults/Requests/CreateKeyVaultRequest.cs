using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.KeyVaults.Requests;

public class CreateKeyVaultRequest : KeyVaultRequestBase
{
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}