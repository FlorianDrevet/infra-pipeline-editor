using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Specifies how a compute resource is deployed: as code or as a container image.</summary>
public sealed class DeploymentMode(DeploymentMode.DeploymentModeType value)
    : EnumValueObject<DeploymentMode.DeploymentModeType>(value)
{
    /// <summary>Available deployment modes.</summary>
    public enum DeploymentModeType
    {
        /// <summary>Traditional code deployment with runtime stack.</summary>
        Code,

        /// <summary>Container-based deployment pulling an image from a registry.</summary>
        Container,
    }
}
