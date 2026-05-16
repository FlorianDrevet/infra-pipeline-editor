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

    /// <summary>Gets the certificate provisioning mode for this custom domain binding.</summary>
    public CertificateMode CertificateMode { get; private set; } = CertificateMode.ManagedCertificate;

    /// <summary>Gets the Azure Key Vault secret URL for the certificate (Key Vault mode only).</summary>
    public string? KeyVaultUrl { get; private set; }

    /// <summary>Gets the resource ID of the managed identity used to access Key Vault (Key Vault mode only).</summary>
    public string? ManagedIdentityResourceId { get; private set; }

    /// <summary>Gets the name of the manually uploaded certificate in the Container App Environment (Manual mode only).</summary>
    public string? CertificateName { get; private set; }

    /// <summary>Gets the DNS validation status for this custom domain binding.</summary>
    public DnsValidationStatus DnsValidationStatus { get; private set; } = DnsValidationStatus.Pending;

    /// <summary>EF Core constructor.</summary>
    private CustomDomain() { }

    /// <summary>Creates a new <see cref="CustomDomain"/> binding.</summary>
    /// <param name="resourceId">Identifier of the parent Azure resource.</param>
    /// <param name="environmentName">The deployment environment name.</param>
    /// <param name="domainName">The fully qualified domain name.</param>
    /// <param name="certificateMode">The certificate provisioning mode.</param>
    /// <param name="keyVaultUrl">Key Vault secret URL (required for Key Vault mode).</param>
    /// <param name="managedIdentityResourceId">Managed identity resource ID (required for Key Vault mode).</param>
    /// <param name="certificateName">Certificate name in the environment (required for Manual mode).</param>
    /// <returns>A new <see cref="CustomDomain"/> entity.</returns>
    internal static CustomDomain Create(
        AzureResourceId resourceId,
        string environmentName,
        string domainName,
        CertificateMode certificateMode,
        string? keyVaultUrl = null,
        string? managedIdentityResourceId = null,
        string? certificateName = null)
        => new()
        {
            Id = CustomDomainId.CreateUnique(),
            ResourceId = resourceId,
            EnvironmentName = environmentName,
            DomainName = domainName.ToLowerInvariant().Trim(),
            CertificateMode = certificateMode,
            KeyVaultUrl = keyVaultUrl,
            ManagedIdentityResourceId = managedIdentityResourceId,
            CertificateName = certificateName,
            DnsValidationStatus = DnsValidationStatus.Pending,
        };

    /// <summary>Marks the DNS records for this domain as validated.</summary>
    public void ValidateDns() => DnsValidationStatus = DnsValidationStatus.Validated;

    /// <summary>Resets the DNS validation status to <see cref="ValueObjects.DnsValidationStatus.Pending"/>.</summary>
    public void ResetDnsValidation() => DnsValidationStatus = DnsValidationStatus.Pending;
}
