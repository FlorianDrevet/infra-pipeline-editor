using InfraFlowSculptor.BicepDirector.Enums;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.Entities;

public sealed class SecretAzureResourceEntity : Entity<SecretAzureResourceEntityId>
{
    public string SecretName { get; protected set; } = null!;
    public string Description { get; protected set; } = null!;
    public ResourceType ResourceType { get; protected set; }
    public string ResourceName { get; protected set; } = null!;

    private SecretAzureResourceEntity(SecretAzureResourceEntityId id, string secretName, string description, ResourceType resourceType, string resourceName)
        : base(id)
    {
        SecretName = secretName;
        Description = description;
        ResourceType = resourceType;
        ResourceName = resourceName;
    }

    public static SecretAzureResourceEntity Create(string name, string description, ResourceType resourceType, string resourceName)
    {
        return new SecretAzureResourceEntity(SecretAzureResourceEntityId.CreateUnique(), name, description, resourceType, resourceName);
    }

    public SecretAzureResourceEntity()
    {
    }
}