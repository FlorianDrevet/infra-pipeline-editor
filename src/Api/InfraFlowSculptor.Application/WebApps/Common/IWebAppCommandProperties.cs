using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.WebApps.Common;

/// <summary>
/// Shared properties between <c>CreateWebAppCommand</c> and <c>UpdateWebAppCommand</c>,
/// used by <see cref="WebAppValidationRules"/> to register common validation rules.
/// </summary>
public interface IWebAppCommandProperties
{
    /// <summary>The resource display name.</summary>
    Name Name { get; }

    /// <summary>The Azure region.</summary>
    Location Location { get; }

    /// <summary>The App Service Plan to host this Web App.</summary>
    Guid AppServicePlanId { get; }

    /// <summary>The runtime stack identifier (e.g. DotNet, Node).</summary>
    string RuntimeStack { get; }

    /// <summary>The runtime version within the selected stack.</summary>
    string RuntimeVersion { get; }

    /// <summary>The deployment mode (Code or Container).</summary>
    string DeploymentMode { get; }

    /// <summary>Optional container registry identifier for container deployments.</summary>
    Guid? ContainerRegistryId { get; }
}
