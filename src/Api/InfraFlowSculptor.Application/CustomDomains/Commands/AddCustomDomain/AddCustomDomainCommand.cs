using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.AddCustomDomain;

/// <summary>Command to add a custom domain binding to a compute resource for a specific environment.</summary>
/// <param name="ResourceId">Identifier of the compute resource (WebApp, FunctionApp, or ContainerApp).</param>
/// <param name="EnvironmentName">The deployment environment name.</param>
/// <param name="DomainName">The fully qualified domain name (e.g. "api.example.com").</param>
/// <param name="CertificateMode">Certificate provisioning mode: "ManagedCertificate", "KeyVaultCertificate", "ManualCertificate", or "Disabled".</param>
/// <param name="KeyVaultUrl">Azure Key Vault secret URL (required for KeyVaultCertificate mode).</param>
/// <param name="ManagedIdentityResourceId">Resource ID of the managed identity for Key Vault access (required for KeyVaultCertificate mode).</param>
/// <param name="CertificateName">Name of the manually uploaded certificate (required for ManualCertificate mode).</param>
public record AddCustomDomainCommand(
    AzureResourceId ResourceId,
    string EnvironmentName,
    string DomainName,
    string CertificateMode = "ManagedCertificate",
    string? KeyVaultUrl = null,
    string? ManagedIdentityResourceId = null,
    string? CertificateName = null) : ICommand<CustomDomainResult>;
