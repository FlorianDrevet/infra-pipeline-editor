using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Represents the certificate provisioning mode for a custom domain binding.</summary>
public sealed class CertificateMode(CertificateMode.CertificateModeType value)
    : EnumValueObject<CertificateMode.CertificateModeType>(value)
{
    /// <summary>Available certificate provisioning modes.</summary>
    public enum CertificateModeType
    {
        /// <summary>Azure auto-provisions and auto-renews a free managed certificate (DigiCert).</summary>
        ManagedCertificate,

        /// <summary>Certificate stored in Azure Key Vault, referenced via managed identity.</summary>
        KeyVaultCertificate,

        /// <summary>User has manually uploaded a PFX certificate to the Container App Environment.</summary>
        ManualCertificate,

        /// <summary>No TLS certificate — HTTP only (domain bound without HTTPS).</summary>
        Disabled,
    }

    /// <summary>Azure auto-provisions and auto-renews a free managed certificate (DigiCert).</summary>
    public static readonly CertificateMode ManagedCertificate = new(CertificateModeType.ManagedCertificate);

    /// <summary>Certificate stored in Azure Key Vault, referenced via managed identity.</summary>
    public static readonly CertificateMode KeyVaultCertificate = new(CertificateModeType.KeyVaultCertificate);

    /// <summary>User has manually uploaded a PFX certificate to the Container App Environment.</summary>
    public static readonly CertificateMode ManualCertificate = new(CertificateModeType.ManualCertificate);

    /// <summary>No TLS certificate — HTTP only (domain bound without HTTPS).</summary>
    public static readonly CertificateMode Disabled = new(CertificateModeType.Disabled);
}
