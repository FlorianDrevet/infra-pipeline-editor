using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="DocumentIntelligence"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class DocumentIntelligenceEnvironmentSettings : Entity<DocumentIntelligenceEnvironmentSettingsId>
{
    /// <summary>Gets the parent Document Intelligence identifier.</summary>
    public AzureResourceId DocumentIntelligenceId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets the pricing tier override.</summary>
    public DocumentIntelligenceSku? Sku { get; private set; }

    /// <summary>Gets the public network access mode override.</summary>
    public PublicNetworkAccessMode? PublicNetworkAccess { get; private set; }

    /// <summary>Gets whether local (key-based) authentication is disabled for this environment.</summary>
    public bool DisableLocalAuth { get; private set; }

    private DocumentIntelligenceEnvironmentSettings() { }

    internal DocumentIntelligenceEnvironmentSettings(
        AzureResourceId documentIntelligenceId,
        string environmentName,
        DocumentIntelligenceSku? sku,
        PublicNetworkAccessMode? publicNetworkAccess,
        bool disableLocalAuth)
        : base(DocumentIntelligenceEnvironmentSettingsId.CreateUnique())
    {
        DocumentIntelligenceId = documentIntelligenceId;
        EnvironmentName = environmentName;
        Sku = sku;
        PublicNetworkAccess = publicNetworkAccess;
        DisableLocalAuth = disableLocalAuth;
    }

    /// <summary>
    /// Creates a new <see cref="DocumentIntelligenceEnvironmentSettings"/> for the specified resource and environment.
    /// </summary>
    public static DocumentIntelligenceEnvironmentSettings Create(
        AzureResourceId documentIntelligenceId,
        string environmentName,
        DocumentIntelligenceSku? sku,
        PublicNetworkAccessMode? publicNetworkAccess,
        bool disableLocalAuth)
        => new(documentIntelligenceId, environmentName, sku, publicNetworkAccess, disableLocalAuth);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        DocumentIntelligenceSku? sku,
        PublicNetworkAccessMode? publicNetworkAccess,
        bool disableLocalAuth)
    {
        Sku = sku;
        PublicNetworkAccess = publicNetworkAccess;
        DisableLocalAuth = disableLocalAuth;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["skuName"] = Sku.Value.ToString();
        if (PublicNetworkAccess is not null) dict["publicNetworkAccess"] = PublicNetworkAccess.Value.ToString();
        if (DisableLocalAuth) dict["disableLocalAuth"] = "true";
        return dict;
    }
}
