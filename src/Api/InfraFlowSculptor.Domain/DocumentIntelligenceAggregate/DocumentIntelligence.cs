using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.Entities;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;

/// <summary>
/// Represents an Azure Document Intelligence resource (<c>Microsoft.CognitiveServices/accounts</c> with kind <c>FormRecognizer</c>).
/// Supports per-environment SKU overrides, network access control, and local auth disable.
/// </summary>
public sealed class DocumentIntelligence : AzureResource
{
    private readonly List<DocumentIntelligenceEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Document Intelligence resource.</summary>
    public IReadOnlyCollection<DocumentIntelligenceEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the custom subdomain name for token-based authentication. Required for Private Endpoint and Entra ID auth.</summary>
    public string? CustomSubDomainName { get; private set; }

    private DocumentIntelligence() { }

    /// <summary>Updates the resource-level properties of this Document Intelligence resource.</summary>
    /// <param name="name">The new resource name.</param>
    /// <param name="location">The new Azure location.</param>
    /// <param name="customSubDomainName">The custom subdomain name for token-based authentication.</param>
    public void Update(
        Name name,
        Location location,
        string? customSubDomainName)
    {
        SetNameAndLocation(name, location);

        if (IsExisting)
            return;

        CustomSubDomainName = customSubDomainName;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        DocumentIntelligenceSku? sku,
        PublicNetworkAccessMode? publicNetworkAccess,
        bool disableLocalAuth)
    {
        if (IsExisting)
            return;

        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, publicNetworkAccess, disableLocalAuth);
        }
        else
        {
            _environmentSettings.Add(
                DocumentIntelligenceEnvironmentSettings.Create(Id, environmentName, sku, publicNetworkAccess, disableLocalAuth));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)> settings)
    {
        if (IsExisting)
            return;

        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                DocumentIntelligenceEnvironmentSettings.Create(Id, s.EnvironmentName, s.Sku, s.PublicNetworkAccess, s.DisableLocalAuth));
        }
    }

    /// <summary>Creates a new <see cref="DocumentIntelligence"/> with a generated identifier.</summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="customSubDomainName">The custom subdomain name for token-based authentication.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <param name="isExisting">When <c>true</c>, this resource already exists in Azure and is not deployed by this project.</param>
    public static DocumentIntelligence Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        string? customSubDomainName,
        IReadOnlyList<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)>? environmentSettings = null,
        bool isExisting = false)
    {
        var docIntel = new DocumentIntelligence
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting,
            CustomSubDomainName = customSubDomainName
        };

        if (!isExisting && environmentSettings is not null)
            docIntel.SetAllEnvironmentSettings(environmentSettings);

        return docIntel;
    }
}
