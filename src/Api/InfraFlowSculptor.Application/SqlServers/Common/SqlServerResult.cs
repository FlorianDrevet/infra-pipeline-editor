using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.SqlServers.Common;

/// <summary>Application-layer result for a SQL Server operation.</summary>
public record SqlServerResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string Version,
    string AdministratorLogin,
    IReadOnlyList<SqlServerEnvironmentConfigData> EnvironmentSettings);
