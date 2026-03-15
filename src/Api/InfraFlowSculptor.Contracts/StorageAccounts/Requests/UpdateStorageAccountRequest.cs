using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

public class UpdateStorageAccountRequest
{
    [Required]
    public required string Name { get; init; }

    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    [Required, EnumValidation(typeof(StorageAccountSku.Sku))]
    public required string Sku { get; init; }

    [Required, EnumValidation(typeof(StorageAccountKind.Kind))]
    public required string Kind { get; init; }

    [Required, EnumValidation(typeof(StorageAccessTier.Tier))]
    public required string AccessTier { get; init; }

    [Required]
    public required bool AllowBlobPublicAccess { get; init; }

    [Required]
    public required bool EnableHttpsTrafficOnly { get; init; }

    [Required, EnumValidation(typeof(StorageAccountTlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }
}
