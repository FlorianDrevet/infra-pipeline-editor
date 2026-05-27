namespace InfraFlowSculptor.Contracts.CustomDomains.Responses;

/// <summary>Response DTO for a custom domain binding.</summary>
/// <param name="Id">Custom domain binding identifier.</param>
/// <param name="ResourceId">Parent Azure resource identifier.</param>
/// <param name="EnvironmentName">Deployment environment name.</param>
/// <param name="DomainName">Fully qualified domain name.</param>
/// <param name="CertificateMode">Certificate provisioning mode ("ManagedCertificate", "KeyVaultCertificate", "ManualCertificate", or "Disabled").</param>
/// <param name="KeyVaultUrl">Azure Key Vault secret URL (Key Vault mode only).</param>
/// <param name="ManagedIdentityResourceId">Managed identity resource ID (Key Vault mode only).</param>
/// <param name="CertificateName">Certificate name in the environment (Manual mode only).</param>
/// <param name="DnsValidationStatus">DNS validation status ("Pending" or "Validated").</param>
public record CustomDomainResponse(
    string Id,
    string ResourceId,
    string EnvironmentName,
    string DomainName,
    string CertificateMode,
    string? KeyVaultUrl,
    string? ManagedIdentityResourceId,
    string? CertificateName,
    string DnsValidationStatus);
