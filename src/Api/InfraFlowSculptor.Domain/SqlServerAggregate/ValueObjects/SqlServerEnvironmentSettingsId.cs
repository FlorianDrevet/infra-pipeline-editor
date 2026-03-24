using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.SqlServerEnvironmentSettings"/>.</summary>
public sealed class SqlServerEnvironmentSettingsId(Guid value) : Id<SqlServerEnvironmentSettingsId>(value);
