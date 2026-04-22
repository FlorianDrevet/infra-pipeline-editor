using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Represents a custom domain name binding configured on a compute resource
/// (Container App, Web App, or Function App) for a specific deployment environment.
/// </summary>
public sealed class CustomDomain : Entity<CustomDomainId>
{
    /// <summary>Gets the identifier of the parent Azure resource.</summary>
    public AzureResourceId ResourceId { get; private set; } = null!;

    /// <summary>Gets the deployment environment name (e.g. "production", "staging").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets the fully qualified domain name (e.g. "api.example.com").</summary>
    public string DomainName { get; private set; } = string.Empty;

    /// <summary>Gets the SSL binding type. Either "SniEnabled" (managed certificate) or "Disabled" (no SSL).</summary>
    public string BindingType { get; private set; } = "SniEnabled";

    /// <summary>EF Core constructor.</summary>
    private CustomDomain() { }

    /// <summary>Creates a new <see cref="CustomDomain"/> binding.</summary>
    /// <param name="resourceId">Identifier of the parent Azure resource.</param>
    /// <param name="environmentName">The deployment environment name.</param>
    /// <param name="domainName">The fully qualified domain name.</param>
    /// <param name="bindingType">The SSL binding type (default: "SniEnabled").</param>
    /// <returns>A new <see cref="CustomDomain"/> entity.</returns>
    internal static CustomDomain Create(
        AzureResourceId resourceId,
        string environmentName,
        string domainName,
        string bindingType = "SniEnabled")
        => new()
        {
            Id = CustomDomainId.CreateUnique(),
            ResourceId = resourceId,
            EnvironmentName = environmentName,
            DomainName = domainName.ToLowerInvariant().Trim(),
            BindingType = bindingType,
        };
}
