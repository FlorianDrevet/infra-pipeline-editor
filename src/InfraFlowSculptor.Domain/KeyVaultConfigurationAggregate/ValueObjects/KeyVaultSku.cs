using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

public class KeyVaultSku : ValueObject
{
    public enum KeyVaultSkuFamilyEnum
    {
        A,
        B,
        C
    }
    
    public enum KeyVaultSkuNameEnum
    {
        Basic,
        Standard,
    }

    public KeyVaultSkuFamilyEnum Family { get; protected set; }
    public KeyVaultSkuNameEnum Name { get; protected set; }

    public KeyVaultSku()
    {
    }

    public KeyVaultSku(KeyVaultSkuFamilyEnum family, KeyVaultSkuNameEnum name)
    {
        this.Family = family;
        this.Name = name;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Family;
        yield return Name;
    }
}