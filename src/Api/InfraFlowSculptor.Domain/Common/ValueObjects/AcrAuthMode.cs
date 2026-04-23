using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Specifies how a compute resource authenticates to Azure Container Registry.</summary>
public sealed class AcrAuthMode(AcrAuthMode.AcrAuthModeType value)
    : EnumValueObject<AcrAuthMode.AcrAuthModeType>(value)
{
    /// <summary>Available ACR authentication modes.</summary>
    public enum AcrAuthModeType
    {
        /// <summary>Uses a managed identity to pull images from Azure Container Registry.</summary>
        ManagedIdentity,

        /// <summary>Uses Azure Container Registry admin credentials to pull images.</summary>
        AdminCredentials,
    }
}