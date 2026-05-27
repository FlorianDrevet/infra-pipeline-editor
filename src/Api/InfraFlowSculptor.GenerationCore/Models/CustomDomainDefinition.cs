namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents a custom domain binding for Bicep generation.
/// </summary>
public class CustomDomainDefinition
{
    /// <summary>Gets or sets the deployment environment name.</summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>Gets or sets the fully qualified domain name.</summary>
    public string DomainName { get; set; } = string.Empty;

    /// <summary>Gets or sets the certificate provisioning mode ("ManagedCertificate", "KeyVaultCertificate", "ManualCertificate", or "Disabled").</summary>
    public string CertificateMode { get; set; } = "ManagedCertificate";

    /// <summary>Gets or sets the Azure Key Vault secret URL (Key Vault mode only).</summary>
    public string? KeyVaultUrl { get; set; }

    /// <summary>Gets or sets the managed identity resource ID for Key Vault access (Key Vault mode only).</summary>
    public string? ManagedIdentityResourceId { get; set; }

    /// <summary>Gets or sets the certificate name in the environment (Manual mode only).</summary>
    public string? CertificateName { get; set; }

    /// <summary>Gets or sets the DNS validation status ("Pending" or "Validated").</summary>
    public string DnsValidationStatus { get; set; } = "Pending";
}
