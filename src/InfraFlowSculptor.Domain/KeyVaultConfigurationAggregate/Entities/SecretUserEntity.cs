using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultConfigurationAggregate.Entities;

public sealed class SecretUserEntity : Entity<SecretUserEntityId>
{
    public string SecretName { get; protected set; } = null!;
    public string Description { get; protected set; } = null!;
    public string ParamName { get; protected set; } = null!;
    
    private SecretUserEntity(SecretUserEntityId id, string secretName, string description, string paramName)
        : base(id)
    {
        SecretName = secretName;
        Description = description;
        ParamName = paramName;
    }

    public static SecretUserEntity Create(string secretName, string description, string paramName)
    {
        return new SecretUserEntity(SecretUserEntityId.CreateUnique(), secretName, description, paramName);
    }
    
    public SecretUserEntity(){}
}