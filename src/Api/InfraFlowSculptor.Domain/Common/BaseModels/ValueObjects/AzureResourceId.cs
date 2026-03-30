using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for an <see cref="AzureResource"/>.</summary>
public class AzureResourceId(Guid value) : Id<AzureResourceId>(value);