using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.CustomDomains.Requests;

/// <summary>Request to add a custom domain binding to a compute resource.</summary>
public class AddCustomDomainRequest
{
    /// <summary>The deployment environment name (e.g. "production", "staging").</summary>
    [Required]
    [MaxLength(100)]
    public required string EnvironmentName { get; init; }

    /// <summary>The fully qualified domain name (e.g. "api.example.com").</summary>
    [Required]
    [MaxLength(253)]
    public required string DomainName { get; init; }

    /// <summary>Certificate provisioning mode: "ManagedCertificate" (default), "KeyVaultCertificate", "ManualCertificate", or "Disabled".</summary>
    [MaxLength(30)]
    public string CertificateMode { get; init; } = "ManagedCertificate";

    /// <summary>Azure Key Vault secret URL (required for KeyVaultCertificate mode).</summary>
    [MaxLength(500)]
    public string? KeyVaultUrl { get; init; }

    /// <summary>Resource ID of the managed identity for Key Vault access (required for KeyVaultCertificate mode).</summary>
    [MaxLength(500)]
    public string? ManagedIdentityResourceId { get; init; }

    /// <summary>Name of the manually uploaded certificate in the Container App Environment (required for ManualCertificate mode).</summary>
    [MaxLength(200)]
    public string? CertificateName { get; init; }
}
