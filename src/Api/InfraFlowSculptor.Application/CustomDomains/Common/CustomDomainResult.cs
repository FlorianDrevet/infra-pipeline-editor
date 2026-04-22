using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CustomDomains.Common;

/// <summary>Application-layer result for a custom domain binding.</summary>
public sealed record CustomDomainResult(
    CustomDomainId Id,
    AzureResourceId ResourceId,
    string EnvironmentName,
    string DomainName,
    string BindingType);
