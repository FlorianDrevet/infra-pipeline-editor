using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.DocumentIntelligences.Requests;

/// <summary>Per-environment configuration for a Document Intelligence resource.</summary>
public class DocumentIntelligenceEnvironmentConfigRequest
{
    /// <summary>Name of the target environment (e.g. "dev", "staging", "prod").</summary>
    public required string EnvironmentName { get; init; }

    /// <summary>Pricing SKU for this environment (F0, S0).</summary>
    [EnumValidation(typeof(DocumentIntelligenceSku.Sku))]
    public string? Sku { get; init; }

    /// <summary>Whether public network access is enabled or disabled.</summary>
    [EnumValidation(typeof(PublicNetworkAccessMode.Mode))]
    public string? PublicNetworkAccess { get; init; }

    /// <summary>Whether to disable local (key-based) authentication.</summary>
    public bool DisableLocalAuth { get; init; }
}
