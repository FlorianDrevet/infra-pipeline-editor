using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.SqlDatabases.Common;

/// <summary>Application-layer result for a SQL Database operation.</summary>
public record SqlDatabaseResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid SqlServerId,
    string Collation,
    IReadOnlyCollection<SqlDatabaseEnvironmentConfigData> EnvironmentSettings);
