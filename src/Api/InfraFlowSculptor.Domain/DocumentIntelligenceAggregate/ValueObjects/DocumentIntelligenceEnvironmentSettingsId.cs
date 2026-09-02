using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.DocumentIntelligenceEnvironmentSettings"/>.</summary>
public sealed class DocumentIntelligenceEnvironmentSettingsId(Guid value) : Id<DocumentIntelligenceEnvironmentSettingsId>(value);
