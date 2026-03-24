using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.SqlDatabaseEnvironmentSettings"/>.</summary>
public sealed class SqlDatabaseEnvironmentSettingsId(Guid value) : Id<SqlDatabaseEnvironmentSettingsId>(value);
