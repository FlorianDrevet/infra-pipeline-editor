using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.KeyVaults.Requests;

public abstract class KeyVaultRequestBase
{
    [Required]
    public required string Name { get; init; }

    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    [Required, EnumValidation(typeof(Sku.SkuEnum))]
    public required string Sku { get; init; }
}
