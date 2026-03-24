using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.LogAnalyticsWorkspaceEnvironmentSettings"/>.</summary>
public sealed class LogAnalyticsWorkspaceEnvironmentSettingsId(Guid value) : Id<LogAnalyticsWorkspaceEnvironmentSettingsId>(value);
